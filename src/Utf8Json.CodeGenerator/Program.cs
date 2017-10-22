using System;
using System.IO;
using System.Text;

namespace Utf8Json.CodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            
        }

        static void Output(string path, string text)
        {
            path = path.Replace("global::", "");

            const string prefix = "[Out]";
            Console.WriteLine(prefix + path);

            var fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            System.IO.File.WriteAllText(path, text, Encoding.UTF8);
        }
    }
}
