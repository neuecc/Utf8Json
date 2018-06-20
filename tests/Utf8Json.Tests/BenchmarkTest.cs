using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class BenchmarkTest
    {

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct LongUnion
        {
            [FieldOffset(0)]
            public int Int1;
            [FieldOffset(1)]
            public int Int2;

            [FieldOffset(0)]
            public float Float;

            [FieldOffset(0)]
            public double Double;

            [FieldOffset(0)]
            public ulong Long;
        }

        public class TargetClass
        {
           
            public sbyte Number1 { get; set; }
           
            public short Number2 { get; set; }
            
            public int Number3 { get; set; }
            
            public long Number4 { get; set; }
            
            public byte Number5 { get; set; }
            
            public ushort Number6 { get; set; }
            
            public uint Number7 { get; set; }
           
            public ulong Number8 { get; set; }

            public static TargetClass Create(Random random)
            {
                unchecked
                {
                    return new TargetClass
                    {
                        Number1 = (sbyte)random.Next(),
                        Number2 = (short)random.Next(),
                        Number3 = (int)random.Next(),
                        Number4 = (long)new LongUnion { Int1 = random.Next(), Int2 = random.Next() }.Long,
                        Number5 = (byte)random.Next(),
                        Number6 = (ushort)random.Next(),
                        Number7 = (uint)random.Next(),
                        Number8 = (ulong)new LongUnion { Int1 = random.Next(), Int2 = random.Next() }.Long,
                    };
                }
            }
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

            public TargetClassContractless()
            {

            }

            public TargetClassContractless(TargetClass tc)
            {
                this.Number1 = tc.Number1;
                this.Number2 = tc.Number2;
                this.Number3 = tc.Number3;
                this.Number4 = tc.Number4;
                this.Number5 = tc.Number5;
                this.Number6 = tc.Number6;
                this.Number7 = tc.Number7;
                this.Number8 = tc.Number8;
            }
        }

        [Fact]
        public void RunBenchmark()
        {
            Debug.WriteLine("Start");

            Console.WriteLine("Start");
            //try
            //{
            //    var rng = new Random(1);
            //    var tgt = TargetClass.Create(rng);
            //    TargetClass tgt1 = null;
            //    var bytes = Utf8Json.JsonSerializer.Serialize(tgt);
            //    long sum = 0;
            //    for (int i = 0; i < 1000000; i++)
            //    {
            //        tgt1 = Utf8Json.JsonSerializer.Deserialize<TargetClass>(bytes);
            //        sum += tgt1.Number4;
            //    }
            //    tgt.IsStructuralEqual(tgt1);
            //    Console.WriteLine(sum);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}
            
        }

        
    }
}
