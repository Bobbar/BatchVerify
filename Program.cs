using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BatchVerify
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the scan procress.
            BatchScanner.Start();

            // Keep console window open until a user keypress.
            Console.ReadKey();
        }
    }
}
