using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace BatchVerify
{
    public static class BatchScanner
    {
        private static List<App> appList = new List<App>();
        private static string scanQPath = @"\\DDAD-SVR-FS02\ScanQ\";

        public static void Start()
        {
            appList = GetAppList();

            foreach (var app in appList)
            {
                Console.WriteLine("App: " + app.Name);
                VerifyApp(app);
            }

            Console.WriteLine("\n \n *********** RESULTS ***********");

            DisplayResults(appList);
        }

        private static void DisplayResults(List<App> apps)
        {
            foreach (var app in apps)
            {
                Console.WriteLine("[Application: " + app.Name + "] \n");
                Console.WriteLine("     Queues:");

                foreach (var queue in app.Queues)
                {
                    Console.WriteLine("     [" + app.Queues.IndexOf(queue) + "]  Name: " + queue.Name + "  ID: " + queue.ID + "  Owner: " + queue.Owner);

                    Console.WriteLine("         Bad Batches:");
                    foreach (var batch in queue.Batches)
                    {
                        // Console.WriteLine("         [" + batch.ID + "]  Pages: " + batch.PageCount + "  Verified: " + batch.Verified + "  Missing: " + batch.MissingFiles.Count);

                        if (!batch.Verified)
                        {
                            Console.WriteLine("         [" + batch.ID + "]  Pages: " + batch.PageCount + "  Verified: " + batch.Verified + "  Missing: " + batch.MissingFiles.Count);

                            if (batch.MissingFiles.Count < batch.PageCount)
                            {
                                Console.WriteLine("             Missing Files:");

                                foreach (var file in batch.MissingFiles)
                                {
                                    Console.WriteLine("             " + file);
                                }
                            }
                            else if (batch.MissingFiles.Count == batch.PageCount)
                            {
                                Console.WriteLine("             ALL FILES MISSING.");
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        private static void VerifyApp(App app)
        {
            for (int q = 0; q < app.Queues.Count; q++)
            {
                var queue = app.Queues[q];
                Console.WriteLine("Verifying Queue: " + queue.QueueTable + " ID:" + queue.ID + " Name: " + queue.Name + " Owner: " + queue.Owner);

                for (int b = 0; b < queue.Batches.Count; b++)
                {
                    Console.WriteLine(queue.Name + " - Batch " + b + " of " + queue.Batches.Count);
                    var batch = queue.Batches[b];

                    bool verified = true;

                    for (int i = 0; i < batch.PageCount; i++)
                    {
                        int seq = i + 1;
                        string fileName = batch.ID.ToString("00000") + seq.ToString("00000");
                        string filePath = scanQPath + "APP" + app.ID.ToString("0000") + @"\" + queue.ID + @".que\" + fileName;

                        if (!File.Exists(filePath))
                        {
                            verified = false;
                            batch.MissingFiles.Add((i + 1) + " - " + fileName);
                        }
                    }

                    batch.Verified = verified;
                }
            }
        }

        private static List<App> GetAppList()
        {
            var tmpList = new List<App>();
            using (var results = IvueConnection.ReturnSqlTable("SELECT * FROM dbo.LSMVolSets"))
            {
                foreach (DataRow row in results.Rows)
                {
                    var newApp = new App(Convert.ToInt32(row["VSID"]), row["Name"].ToString());
                    PopulateQueues(newApp);
                    PopulateBatches(newApp);
                    tmpList.Add(newApp);
                }
            }

            return tmpList;
        }

        private static void PopulateQueues(App app)
        {
            string queueTable = "dbo.SQ$" + app.ID.ToString("0000");

            using (var results = IvueConnection.ReturnSqlTable("SELECT * FROM " + queueTable))
            {
                foreach (DataRow row in results.Rows)
                {
                    var newQueue = new ScanQueue(queueTable, row["queuename"].ToString(), Convert.ToInt32(row["queueid"]), row["owner"].ToString());
                    app.Queues.Add(newQueue);
                }
            }
        }

        private static void PopulateBatches(App app)
        {
            foreach (var queue in app.Queues)
            {
                string batchTable = "[" + queue.Owner + "].[SB$" + app.ID.ToString("0000") + "$" + queue.ID.ToString("0000") + "]";

                try
                {
                    string query = "SELECT * FROM " + batchTable;
                    using (var results = IvueConnection.ReturnSqlTable(query))
                    {
                        foreach (DataRow row in results.Rows)
                        {
                            var newBatch = new Batch(batchTable, Convert.ToInt32(row["batchid"]), Convert.ToInt32(row["pages"]), row["scandex"].ToString());
                            queue.Batches.Add(newBatch);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}