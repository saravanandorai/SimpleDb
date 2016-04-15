using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using System.Globalization;
using System.Collections.Specialized;
using System.Diagnostics;

namespace SimpleDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.WriteLine(string.Format("Test logging message {0}", 0));
            Trace.Write(string.Format("Test logging message {0}", 0));
            Trace.TraceError("hello");
            Trace.TraceInformation("hello");
            Trace.TraceWarning("hello");
            Console.ReadKey();
        }
    }
}
