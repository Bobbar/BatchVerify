using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchVerify
{
    public class Batch
    {
        public string BatchTable { get; set; }

        public int ID { get; set; }

        public int PageCount { get; set; }

        public string Name { get; set; }

        public bool Verified { get; set; }

        public List<string> MissingFiles { get; set; }

        public Batch(string batchTable, int id, int pageCount, string name)
        {
            BatchTable = batchTable;
            ID = id;
            PageCount = pageCount;
            Name = name;
            Verified = false;
            MissingFiles = new List<string>();
        }

    }
}
