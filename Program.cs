﻿using System;


namespace BatchVerify
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the scan procress.
            Scanning.BatchScanner.Start();

            // Keep console window open until a user keypress.
            Console.ReadKey();
        }
    }
}
