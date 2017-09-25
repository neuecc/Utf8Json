using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;
using Utf8Json.Resolvers.Internal;

namespace DynamicCodeDumper
{
    public class Program
    {
        static void Main(string[] args)
        {
#if DUMP_ASM

            try
            {
                //DynamicObjectResolver.Default.GetFormatter<Test>();
                //DynamicObjectResolver.Default.GetFormatter<Test2>();
                //DynamicObjectResolver.Default.GetFormatter<TargetClassContractless>();
                DynamicObjectResolver.Default.GetFormatter<SimplePerson>();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                var a1 = (DynamicObjectResolver.Default as ISave).Save();
                //var a2 = DynamicUnionResolver.Instance.Save();
                //var a3 = DynamicEnumResolver.Instance.Save();
                //var a4 = DynamicContractlessObjectResolver.Instance.Save();

                Verify(a1);
            }

#endif
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


    public class Test
    {
        public int MyProperty1 { get; set; }
        public int MyProperty2 { get; set; }
        public Test2 MyProperty3 { get; set; }
    }

    public class Test2
    {
        public string MyProperty { get; set; }
    }

    public class TargetClassContractless
    {
        public sbyte Number1 { get; set; }
        public short Number2 { get; set; }
        public int Number3 { get; set; }
        public long Number4 { get; set; }
        public byte Number5 { get; set; }
        public ushort Number6 { get; set; }
        public uint Number7 { get; set; }
        public ulong Number8 { get; set; }
        //public float Number9 { get; set; }
        //public double Number10 { get; set; }
        public string Str { get; set; }
        public int[] Array { get; set; }
    }

    public class SimplePerson
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        [JsonFormatter(typeof(FooFormatter))]
        public string LastName { get; set; }
    }

    public class FooFormatter : IJsonFormatter<int>
    {
        public int Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
        }

        public void Serialize(ref JsonWriter writer, int value, IJsonFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
        }
    }
}