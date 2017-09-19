using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Internal.DoubleConversion;
using Xunit;

namespace Utf8Json.Tests
{
    public class DoubleConversionTest
    {
        [Fact]
        public unsafe void Dtoa()
        {
            //var bytes = new byte[100];
            //long len;
            //fixed (byte* buf = &bytes[0])
            //{
            //    var huga = DoubleConversion.dtoa(12346.0D, buf);
            //    len = huga - buf;
            //}

            //var test = Encoding.UTF8.GetString(bytes, 0, (int)len);
        }
    }
}
