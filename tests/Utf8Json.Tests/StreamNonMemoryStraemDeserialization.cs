using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class StreamNonMemoryStraemDeserializationTest
    {
        [Fact]
        public void Foo()
        {
            var ms = new NonMemoryStream(new MemoryStream(new byte[] { (byte)'9' }));
            var d = JsonSerializer.Deserialize<int>(ms);
            d.Is(9);
        }
    }

    class NonMemoryStream : Stream
    {
        MemoryStream ms;

        public NonMemoryStream(MemoryStream ms)
        {
            this.ms = ms;
        }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            ms.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ms.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ms.Write(buffer, offset, count);
        }
    }
}
