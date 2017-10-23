﻿using Xunit;

namespace Utf8Json.Tests
{
    public class ValueTupleTest
    {
        T Convert<T>(T value)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
        }

        public static object valueTupleData = new object[]
        {
            new object[] { (1, 2) },
            new object[] { (1, 2, 3) },
            new object[] { (1, 2, 3, 4) },
            new object[] { (1, 2, 3, 4, 5) },
            new object[] { (1, 2, 3, 4, 5,6) },
            new object[] { (1, 2, 3, 4, 5,6,7) },
            new object[] { (1, 2, 3, 4, 5,6,7,8) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14,15) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14,15,16) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14,15,16,17) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14,15,16,17,18) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14,15,16,17,18,19) },
            new object[] { (1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20) },
        };

        [Theory]
        [MemberData(nameof(valueTupleData))]
        public void ValueTuple<T>(T x)
        {
            Convert(x).Is(x);
        }
    }
}
