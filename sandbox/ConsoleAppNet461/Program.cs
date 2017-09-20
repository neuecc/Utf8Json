using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utf8Json.Internal.DoubleConversion;

namespace ConsoleAppNet461
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            var buffer = new byte[100];


            var s = DoubleToStringConverter.GetString(123.456);
            Console.WriteLine(s);
        }
    }
}
