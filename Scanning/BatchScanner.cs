using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BatchVerify.Containers;
using BatchVerify.Data;
using BatchVerify.UI;

namespace BatchVerify.Scanning
{
    public static class BatchScanner
    {
        private static List<App> appList = new List<App>();
        private static string scanQPath = @"\\DDAD-SVR-FS02\ScanQ\";

        private static long totalItems = 0;
        private static long scannedItems = 0;
        private static long lastUpdate = 0;

        private static string consoleTitle = "Scanning Batches: ";

        public static void Start()
        {
            // Check for previous scan results and prompt user
            // to reload or start a new scan.
            bool reloadPrevious = false;

            if (HasPreviousScan())
            {
                Console.WriteLine("Reload previous scan? (Y/N): ");
                var prompt = Console.ReadLine();

                if (prompt.ToUpper().Trim() == "Y")
                {
                    reloadPrevious = true;
                }
            }

            if (!reloadPrevious)
            {
                StartNewScan();
            }
            else
            {
                // If reloading, deserialize the results from disk and populate the app list.
                appList = Serializer.DeSerializeAppList();
            }

            //  DisplayResultsConsole(appList);

            DisplayResultsNodeTree(appList);
        }

        private static void StartNewScan()
        {
            var startTime = DateTime.Now.Ticks;

            // Populate the app list from DB.
            appList = GetAppList();

            // Set total items for progress tracking.
            totalItems = CalcTotalItems(appList);

            // Iterate through apps and verify batches.
            foreach (var app in appList)
            {
                VerifyApp(app);
            }

            // Serialize the results to disk for later viewing.
            Serializer.SerializeAppList(appList);

            var elapTime = (DateTime.Now.Ticks - startTime) / 10000 / 1000;
            Console.WriteLine(totalItems + " files scanned in " + elapTime + " seconds.");
        }

        /// <summary>
        /// Returns true if a previous serialized scan result is present in the current working directory.
        /// </summary>
        /// <returns></returns>
        private static bool HasPreviousScan()
        {
            return File.Exists(Serializer.ScanDataFilepath);
        }

        /// <summary>
        /// Prints the results to the console window.
        /// </summary>
        /// <param name="apps"></param>
        private static void DisplayResultsConsole(List<App> apps)
        {
            Console.WriteLine("\n \n *********** RESULTS ***********");

            foreach (var app in apps)
            {
                // App list.
                Console.WriteLine("[Application: " + app.Name + "] \n");
                Console.WriteLine("     Queues:");

                foreach (var queue in app.Queues)
                {
                    // Queue list.
                    Console.WriteLine("     [" + app.Queues.IndexOf(queue) + "]  Name: " + queue.Name + "  ID: " + queue.ID + "  Owner: " + queue.Owner);
                    Console.WriteLine("         Bad Batches:");

                    foreach (var batch in queue.Batches)
                    {
                        // Batch list.

                        // If the batch failed verification, include the list of missing files.
                        if (!batch.Verified)
                        {
                            // Print the batch info.
                            Console.WriteLine("         [" + batch.ID + "]  Pages: " + batch.PageCount + "  Verified: " + batch.Verified + "  Missing: " + batch.MissingFiles.Count);

                            // Print the missing file list if only some of the files are missing.
                            if (batch.MissingFiles.Count < batch.PageCount)
                            {
                                Console.WriteLine("             Missing Files:");

                                foreach (var file in batch.MissingFiles)
                                {
                                    Console.WriteLine("             " + file);
                                }
                            }
                            // Don't list missing files if ALL of them are missing.
                            else if (batch.MissingFiles.Count == batch.PageCount)
                            {
                                Console.WriteLine("             ALL FILES MISSING.");
                            }
                        }
                        else
                        {
                            // Print only the batch info if it was verified as correct.
                            // Console.WriteLine("         [" + batch.ID + "]  Pages: " + batch.PageCount + "  Verified: " + batch.Verified + "  Missing: " + batch.MissingFiles.Count);
                        }
                    }
                }
                Console.WriteLine("\n");
            }
        }

        /// <summary>
        /// Displays the scan results in a TreeView control and form.
        /// </summary>
        /// <param name="apps"></param>
        private static void DisplayResultsNodeTree(List<App> apps)
        {
            var nodeTree = new NodeTreeForm();

            nodeTree.DisplayAppTree(apps);
        }

        /// <summary>
        /// Iterates through all batches and calculates the total number of files to be scanned.
        /// </summary>
        /// <param name="apps"></param>
        /// <returns></returns>
        private static long CalcTotalItems(List<App> apps)
        {
            long items = 0;

            foreach (var app in apps)
            {
                foreach (var queue in app.Queues)
                {
                    foreach (var batch in queue.Batches)
                    {
                        items += batch.PageCount;
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Calculates and displays the current progress in the Console window title.
        /// </summary>
        private static void DisplayProgress()
        {
            if (scannedItems <= 0) return;

            // Logic to limit the rate of updates.
            var minUpdateRate = 100; // Minimum milliseconds between updates.
            var currentTick = DateTime.Now.Ticks;

            if (lastUpdate == 0) lastUpdate = currentTick;

            var elapTime = (currentTick - lastUpdate) / 10000;

            if (elapTime >= minUpdateRate | scannedItems == totalItems)
            {
                int percent = (int)((scannedItems / (float)totalItems) * 100);
                Console.Title = consoleTitle + percent + "%";
                lastUpdate = currentTick;
            }
        }

        /// <summary>
        /// Iterates through each queue and batches within the specified App and verifies that the files on disk are present for each batch file.
        /// </summary>
        /// <param name="app"></param>
        private static void VerifyApp(App app)
        {
            Console.WriteLine("Verifying App: " + app.Name);

            // Iterate queues.
            for (int q = 0; q < app.Queues.Count; q++)
            {
                var queue = app.Queues[q];

                Console.WriteLine("Verifying Queue: " + queue.QueueTable + " ID:" + queue.ID + " Name: " + queue.Name + " Owner: " + queue.Owner);

                // Iterate batches in parallel.
                Parallel.For(0, queue.Batches.Count,
                    index =>
                    {
                        var batch = queue.Batches[index];

                        bool verified = true;

                        // Iterate batch pages.
                        for (int i = 0; i < batch.PageCount; i++)
                        {
                            // Non-zero based file index.
                            // Build the filename and paths.
                            int fileIdx = i + 1;
                            string fileName = batch.ID.ToString("00000") + fileIdx.ToString("00000");
                            string filePath = scanQPath + "APP" + app.ID.ToString("0000") + @"\" + queue.ID + @".que\" + fileName;

                            // Check if the file on disk is present.
                            if (!File.Exists(filePath))
                            {
                                // If not, set the verified bool and add the file info to a list.
                                verified = false;
                                batch.MissingFiles.Add("[" + fileIdx + "] - " + fileName);
                            }

                            // Increment scanned items for progress tracking.
                            Interlocked.Increment(ref scannedItems);
                        }

                        // Set the batch verified bool.
                        batch.Verified = verified;

                        // Update progress.
                        DisplayProgress();
                    });
            }
        }

        /// <summary>
        /// Returns a list of App instances populated from the database.
        /// </summary>
        /// <returns></returns>
        private static List<App> GetAppList()
        {
            // List of Apps.
            var tmpList = new List<App>();

            // Query the DB for the collection of applications.
            using (var results = IvueConnection.ReturnSqlTable("SELECT * FROM dbo.LSMVolSets"))
            {
                foreach (DataRow row in results.Rows)
                {
                    // Create and populate a new App instance.
                    var newApp = new App(Convert.ToInt32(row["VSID"]), row["Name"].ToString());

                    // Populate the new instance with Queues.
                    PopulateQueues(newApp);

                    // Populate the new instance with Batches.
                    PopulateBatches(newApp);

                    // Add the new insance to the list.
                    tmpList.Add(newApp);
                }
            }

            // Return the fully populated App list.
            return tmpList;
        }

        private static void PopulateQueues(App app)
        {
            // Build table name from App ID.
            string queueTable = "dbo.SQ$" + app.ID.ToString("0000");

            // Query the DB for queues.
            using (var results = IvueConnection.ReturnSqlTable("SELECT * FROM " + queueTable + " ORDER BY queueid"))
            {
                foreach (DataRow row in results.Rows)
                {
                    // Create and populate new queue instances.
                    var newQueue = new ScanQueue(queueTable, row["queuename"].ToString(), Convert.ToInt32(row["queueid"]), row["owner"].ToString());
                    app.Queues.Add(newQueue);
                }
            }
        }

        private static void PopulateBatches(App app)
        {
            foreach (var queue in app.Queues)
            {
                // Build batch table name.
                string batchTable = "[" + queue.Owner + "].[SB$" + app.ID.ToString("0000") + "$" + queue.ID.ToString("0000") + "]";

                try
                {
                    string query = "SELECT * FROM " + batchTable + " ORDER BY batchid";
                    using (var results = IvueConnection.ReturnSqlTable(query))
                    {
                        foreach (DataRow row in results.Rows)
                        {
                            // Create and populate new batch instances.
                            var newBatch = new Batch(batchTable, Convert.ToInt32(row["batchid"]), Convert.ToInt32(row["pages"]), row["scandex"].ToString());
                            queue.Batches.Add(newBatch);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Write errors to the console.
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}