using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchVerify
{
    public class App
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public List<ScanQueue> Queues { get; set; }

        public App(int id, string name)
        {
            ID = id;
            Name = name;
            Queues = new List<ScanQueue>();
        }
    }
}
