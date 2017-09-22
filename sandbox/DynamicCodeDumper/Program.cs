using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicCodeDumper
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                //var a1 = DynamicObjectResolver.Instance.Save();
                //var a2 = DynamicUnionResolver.Instance.Save();
                //var a3 = DynamicEnumResolver.Instance.Save();
                //var a4 = DynamicContractlessObjectResolver.Instance.Save();

                // Verify(a1);
            }
        }

        static void Verify(params AssemblyBuilder[] builders)
        {
            var path = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\PEVerify.exe";

            foreach (var targetDll in builders)
            {
                var psi = new ProcessStartInfo(path, targetDll.GetName().Name + ".dll")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                var p = Process.Start(psi);
                var data = p.StandardOutput.ReadToEnd();
                Console.WriteLine(data);
            }
        }
    }
}