using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BatchVerify
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
                var appNode = new TreeNode(app.Name);

                foreach (var queue in app.Queues)
                {
                    var queueNode = new TreeNode("QueueID: " + queue.ID + "  Name: " + queue.Name + "  Owner: " + queue.Owner + "  Batch Count: " + queue.Batches.Count);
                    queueNode.BackColor = Color.DarkGreen;

                    if (HasBadBatches(queue))
                    {
                        queueNode.BackColor = Color.DarkRed;
                    }

                    foreach (var batch in queue.Batches)
                    {
                        var batchNode = new TreeNode("[" + batch.ID + "]  Name: " + batch.Name + "  Pages: " + batch.PageCount + "  Missing: " + batch.MissingFiles.Count + "  Verified: " + batch.Verified);
                        batchNode.BackColor = Color.DarkGreen; // Default back color for "Good" batches.

                        if (!batch.Verified)
                        {
                            batchNode.BackColor = Color.DarkRed; // Set "Bad" batches to red back color.

                            if (batch.MissingFiles.Count < batch.PageCount)
                            {
                                var fileNode = new TreeNode("Missing Files (" + batch.MissingFiles.Count + ")");

                                foreach (var file in batch.MissingFiles)
                                {
                                    fileNode.Nodes.Add(new TreeNode(file));
                                }

                                batchNode.Nodes.Add(fileNode);
                            }
                            else if (batch.MissingFiles.Count == batch.PageCount)
                            {
                                batchNode.Nodes.Add(new TreeNode("ALL FILES MISSING"));
                            }
                        }

                        queueNode.Nodes.Add(batchNode);
                    }

                    appNode.Nodes.Add(queueNode);
                }

                topNode.Nodes.Add(appNode);
            }

            NodeTree.Nodes.Add(topNode);
            NodeTree.Nodes[0].Expand();

            this.ShowDialog();
        }

        private bool HasBadBatches(ScanQueue queue)
        {
            return (queue.Batches.FindAll(b => !b.Verified).Count > 0);
        }
    }
}