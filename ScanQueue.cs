using System;
using System.Collections.Generic;

namespace BatchVerify
{
    [Serializable()]
    public class ScanQueue
    {
        public string QueueTable { get; set; }

        public string Name { get; set; }

        public int ID { get; set; }

        public string Owner { get; set; }

        public List<Batch> Batches { get; set; }

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
        }
    }
}