using System;
using Xunit;

namespace Utf8Json.Tests
{
    public class TimeSpanTest
    {
        [Fact]
        public void TimeSpan_Serialize()
        {
            JsonSerializer.ToJsonString(TimeSpan.FromDays(8.5)).Is("\"8.12:00:00\"");
            JsonSerializer.ToJsonString(TimeSpan.FromDays(7)).Is("\"7.00:00:00\"");
            JsonSerializer.ToJsonString(TimeSpan.FromHours(2.5)).Is("\"02:30:00\"");
            JsonSerializer.ToJsonString(TimeSpan.FromMilliseconds(999)).Is("\"00:00:00.9990000\"");
            JsonSerializer.ToJsonString(TimeSpan.FromMilliseconds(99)).Is("\"00:00:00.0990000\"");
            JsonSerializer.ToJsonString(TimeSpan.FromMilliseconds(9)).Is("\"00:00:00.0090000\"");
            JsonSerializer.ToJsonString(TimeSpan.FromTicks(2)).Is("\"00:00:00.0000002\"");
            JsonSerializer.ToJsonString(TimeSpan.FromTicks(1)).Is("\"00:00:00.0000001\"");
            JsonSerializer.ToJsonString(TimeSpan.FromDays(7).Add(TimeSpan.FromTicks(1))).Is("\"7.00:00:00.0000001\"");
        }
        
        [Fact]
        public void TimeSpan_Deserialize()
        {
            JsonSerializer.Deserialize<TimeSpan>("\"8.12:00:00\"").Is(TimeSpan.FromDays(8.5));
            JsonSerializer.Deserialize<TimeSpan>("\"7.00:00:00\"").Is(TimeSpan.FromDays(7));
            JsonSerializer.Deserialize<TimeSpan>("\"02:30:00\"").Is(TimeSpan.FromHours(2.5));
            JsonSerializer.Deserialize<TimeSpan>("\"00:00:00.9990000\"").Is(TimeSpan.FromMilliseconds(999));
            JsonSerializer.Deserialize<TimeSpan>("\"00:00:00.0990000\"").Is(TimeSpan.FromMilliseconds(99));
            JsonSerializer.Deserialize<TimeSpan>("\"00:00:00.0090000\"").Is(TimeSpan.FromMilliseconds(9));
            JsonSerializer.Deserialize<TimeSpan>("\"00:00:00.0000002\"").Is(TimeSpan.FromTicks(2));
            JsonSerializer.Deserialize<TimeSpan>("\"00:00:00.0000001\"").Is(TimeSpan.FromTicks(1));
            JsonSerializer.Deserialize<TimeSpan>("\"7.00:00:00.0000001\"").Is(TimeSpan.FromDays(7).Add(TimeSpan.FromTicks(1)));
        }

        [Fact]
        public void TimeSpan_Field_Serialize()
        {
            JsonSerializer.ToJsonString(new TimeSpanWrapper{Value = TimeSpan.FromDays(7)}).Is("{\"Value\":\"7.00:00:00\"}");
            JsonSerializer.ToJsonString(new TimeSpanWrapper{Value = TimeSpan.FromDays(8.5)}).Is("{\"Value\":\"8.12:00:00\"}");
            JsonSerializer.ToJsonString(new TimeSpanWrapper{Value = TimeSpan.FromDays(7).Add(TimeSpan.FromTicks(1))}).Is("{\"Value\":\"7.00:00:00.0000001\"}");
        }

        [Fact]
        public void TimeSpan_Field_Deserialize()
        {
            JsonSerializer.Deserialize<TimeSpanWrapper>("{\"Value\":\"8.12:00:00\"}").Value.Is(TimeSpan.FromDays(8.5));
            JsonSerializer.Deserialize<TimeSpanWrapper>("{\"Value\":\"7.00:00:00\"}").Value.Is(TimeSpan.FromDays(7));
            JsonSerializer.Deserialize<TimeSpanWrapper>("{\"Value\":\"7.00:00:00.0000001\"}").Value.Is(TimeSpan.FromDays(7).Add(TimeSpan.FromTicks(1)));
        }

        public class TimeSpanWrapper
        {
            public TimeSpan Value;
        }
    }
}