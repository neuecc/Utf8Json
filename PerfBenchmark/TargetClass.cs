using MessagePack;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PerfBenchmark
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

    [MessagePackObject]
    [ProtoBuf.ProtoContract]
    public class TargetClass
    {
        [Key(0)]
        [ProtoBuf.ProtoMember(1)]
        public sbyte Number1 { get; set; }
        [Key(1)]
        [ProtoBuf.ProtoMember(2)]
        public short Number2 { get; set; }
        [Key(2)]
        [ProtoBuf.ProtoMember(3)]
        public int Number3 { get; set; }
        [Key(3)]
        [ProtoBuf.ProtoMember(4)]
        public long Number4 { get; set; }
        [Key(4)]
        [ProtoBuf.ProtoMember(5)]
        public byte Number5 { get; set; }
        [Key(5)]
        [ProtoBuf.ProtoMember(6)]
        public ushort Number6 { get; set; }
        [Key(6)]
        [ProtoBuf.ProtoMember(7)]
        public uint Number7 { get; set; }
        [Key(7)]
        [ProtoBuf.ProtoMember(8)]
        public ulong Number8 { get; set; }
        [Key(8)]
        [ProtoBuf.ProtoMember(9)]
        public float Number9 { get; set; }
        [Key(9)]
        [ProtoBuf.ProtoMember(10)]
        public double Number10 { get; set; }
        [Key(10)]
        [ProtoBuf.ProtoMember(11)]
        public string Str { get; set; }
        [Key(11)]
        [ProtoBuf.ProtoMember(12)]
        public int[] Array { get; set; }

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
                    Number9 = (float)new LongUnion { Int1 = random.Next(), Int2 = random.Next() }.Float,
                    Number10 = (double)new LongUnion { Int1 = random.Next(), Int2 = random.Next() }.Double,
                    Str = "FooBarBazBaz",
                    Array = new[] { 1, 10, 100, 1000, 10000, 100000 }
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
        public float Number9 { get; set; }
        public double Number10 { get; set; }
        public string Str { get; set; }
        public int[] Array { get; set; }

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
            this.Number9 = tc.Number9;
            this.Number10 = tc.Number10;
            this.Str = tc.Str;
            this.Array = tc.Array;
        }
    }

}
