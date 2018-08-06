using System;
using System.Collections.Generic;

namespace BatchVerify.Containers
{
    [Serializable()]
    public class ScanQueue
    {
        public string QueueTable { get; set; }

        public string Name { get; set; }

        public int ID { get; set; }

        public string Owner { get; set; }

        public List<Batch> Batches { get; set; }

        public List<Batch> OrphanBatches { get; set; }

        public ScanQueue()
        {
        }

        public ScanQueue(string queueTable, string name, int id, string owner)
        {
            QueueTable = queueTable;
            Name = name;
            ID = id;
            Owner = owner;
            Batches = new List<Batch>();
            OrphanBatches = new List<Batch>();
        }

        public int BadBatchCount()
        {
            return Batches.FindAll(b => !b.Verified).Count;
        }
    }
}