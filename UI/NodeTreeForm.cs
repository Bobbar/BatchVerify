using BatchVerify.Containers;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BatchVerify.Scanning;
using System.Diagnostics;

namespace BatchVerify.UI
{
    public partial class NodeTreeForm : Form
    {
        public NodeTreeForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Builds and displays a node tree containing the specified results.
        /// </summary>
        /// <param name="apps"></param>
        public void DisplayAppTree(List<App> apps)
        {
            var topNode = new TreeNode("Applications");

            foreach (var app in apps)
            {
                // Create a Application node.
                var appNode = new TreeNode(string.Format("{0} [{1}]", app.Name, app.ID));

                // Iterate the queues within the application.
                foreach (var queue in app.Queues)
                {
                    // Create a queue node.
                    var queueNode = new TreeNode("QueueID: " + queue.ID + "  Name: " + queue.Name + "  Owner: " + queue.Owner + "  Batch Count: " + queue.Batches.Count + "  Bad Batches: " + queue.BadBatchCount() + "  Orphans: " + queue.OrphanBatches.Count);
                    queueNode.BackColor = Color.DarkGreen;

                    // Create a root batch node.
                    var rootBatchNode = new TreeNode("Batches (" + queue.Batches.Count + ")");

                    // Iterate the batches in the queue.
                    foreach (var batch in queue.Batches)
                    {
                        // Create a parent batch node.
                        var batchNode = new TreeNode("[" + batch.ID + "]  Name: " + batch.Name + "  Pages: " + batch.PageCount + "  Missing: " + batch.MissingFiles.Count + "  Orphans: " + batch.OrphanFiles.Count + "  Verified: " + batch.Verified);
                        batchNode.BackColor = Color.DarkGreen; // Default back color for "Good" batches.

                        // If the batch was not verified or has orphan files,
                        // we add child nodes containing the missing files and/or orphans.
                        if (!batch.Verified || batch.OrphanFiles.Count > 0)
                        {
                            if (!batch.Verified)
                            {
                                // Set "Bad" batches and queues to red back color.
                                queueNode.BackColor = Color.DarkRed;
                                batchNode.BackColor = Color.DarkRed;
                            }
                            else if (batch.Verified && batch.OrphanFiles.Count > 0)
                            {
                                // If batch only has orphans, set to a yellow back color.
                                batchNode.BackColor = Color.DarkGoldenrod;
                            }

                            // MISSING FILE NODE:
                            // If the batch is only missing some files.
                            if (batch.MissingFiles.Count < batch.PageCount)
                            {
                                // Create and populate a missing file node.
                                var fileNode = new TreeNode("Missing Files (" + batch.MissingFiles.Count + ")");
                                batch.MissingFiles.ForEach(f => fileNode.Nodes.Add(new TreeNode(f)));

                                // Add the missing file node to the parent batch node.
                                batchNode.Nodes.Add(fileNode);
                            }
                            else if (batch.MissingFiles.Count == batch.PageCount)
                            {
                                // If the batch is missing ALL files, just add a single "all missing" node.
                                batchNode.Nodes.Add(new TreeNode("ALL FILES MISSING"));
                            }

                            // ORPHAN FILE NODE:
                            // If the batch has orphan files.
                            if (batch.OrphanFiles.Count > 0)
                            {
                                // Create and populate a orphan file node.
                                var orphanFileNode = new TreeNode("Orphan Files (" + batch.OrphanFiles.Count + ")");

                                // Add the filepath to the node tag to allow later commands to open and view the file.
                                foreach (var file in batch.OrphanFiles)
                                {
                                    var childNode = new TreeNode(file);
                                    childNode.Tag = BatchScanner.GetFilePath(file, app, queue);
                                    orphanFileNode.Nodes.Add(childNode);
                                }

                                // Add the orphan file node to the parent batch node.
                                batchNode.Nodes.Add(orphanFileNode);
                            }
                        }

                        // Add the parent batch node to the higher root.
                        rootBatchNode.Nodes.Add(batchNode);
                    }

                    // Add the root batch node to the queue node.
                    queueNode.Nodes.Add(rootBatchNode);

                    // ORPHAN BATCHES NODE:
                    // If the queue has orphan batches.
                    if (queue.OrphanBatches.Count > 0)
                    {
                        // Set the parent queue node to red back color.
                        queueNode.BackColor = Color.DarkRed;

                        // Create and populate a orphan batch node.
                        var orphanBatchNode = new TreeNode("Orphan Batches (" + queue.OrphanBatches.Count + ")");
                        queue.OrphanBatches.ForEach(b => orphanBatchNode.Nodes.Add(new TreeNode("[" + b.ID + "]  Pages: " + b.PageCount)));

                        // Add the orphan batch node to the parent queue node.
                        queueNode.Nodes.Add(orphanBatchNode);
                    }

                    // Add the completed queue node to the app node.
                    appNode.Nodes.Add(queueNode);
                }

                // Add the completed app node to the topmost "top" node.
                topNode.Nodes.Add(appNode);
            }

            // Add the completed tree node to the tree control.
            NodeTree.Nodes.Add(topNode);
            NodeTree.Nodes[0].Expand();

            this.ShowDialog();
        }

        private void NodeTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Node.Tag != null && e.Node.Tag is string)
                {
                    Process.Start("mspaint.exe", e.Node.Tag.ToString());
                }
            }
            catch
            {
                // Don't care.
            }
          
        }
    }
}