using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class DateAndTime
    {
        [Fact]
        public void DateTimeOffsetTest()
        {
            DateTimeOffset now = new DateTime(DateTime.UtcNow.Ticks + TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time").BaseUtcOffset.Ticks, DateTimeKind.Local);
            var binary = JsonSerializer.Serialize(now);
            JsonSerializer.Deserialize<DateTimeOffset>(binary).ToString().Is(now.ToString());


            //foreach (var item in new [] { TimeSpan.MaxValue, TimeSpan.MinValue })
            //{
            //    var ts = JsonSerializer.Deserialize<TimeSpan>(JsonSerializer.Serialize(item));
            //}
            //new object[] { DateTimeOffset.MaxValue, DateTimeOffset.MinValue, null },

            foreach (var item in new[] { DateTimeOffset.MinValue.ToUniversalTime(), DateTimeOffset.MaxValue.ToUniversalTime() })
            {
                var ts = JsonSerializer.Deserialize<DateTime>(JsonSerializer.Serialize(item));
            }
            //new object[] { DateTime.MinValue.ToUniversalTime(), DateTime.MaxValue.ToUniversalTime() },
            //new object[] { (DateTime?)DateTime.UtcNow, null },
        }

        [Fact]
        public void Nullable()
        {
            DateTimeOffset? now = new DateTime(DateTime.UtcNow.Ticks + TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time").BaseUtcOffset.Ticks, DateTimeKind.Local);
            var binary = JsonSerializer.Serialize(now);
            JsonSerializer.Deserialize<DateTimeOffset?>(binary).ToString().Is(now.ToString());
        }
    }

}
