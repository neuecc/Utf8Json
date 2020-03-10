using System;
using System.IO;
using System.Text;
using Utf8Json.Formatters;
using Utf8Json.Resolvers;
using Xunit;

namespace Utf8Json.Tests
{
    public class Iso8601DurationTest
    {
        static Iso8601DurationTest()
        {
            CompositeResolver.RegisterAndSetAsDefault(
                new IJsonFormatter[] { Iso8601TimeSpanFormatter.Default },
                new[] { StandardResolver.Default });
        }

        [Fact]
        public void Deserialize_ReturnsTimeSpanZero_WhenStringEqualsToPT0S()
        {
            // Arrange
            const string duration = "{ \"Test\": \"T0S\" }";
            var expected = TimeSpan.Zero;

            // Act
            var wrapper = JsonSerializer.Deserialize<TestWrapper>(duration);
            var actual = wrapper.Test;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(2, 23, 40, 40)]
        [InlineData(200, 200, 200, 200)]
        [InlineData(0, 200, 0, 200)]
        [InlineData(200, 200, 0, 0)]
        public void
            Deserialize_ReturnsExpectedTimeSpan_WhenStringIsValidIso8601Duration(
                int days,
                int hours,
                int minutes,
                int seconds)
        {
            // Arrange
            var duration = $"{{ \"Test\": \"P{days}DT{hours}H{minutes}M{seconds}S\" }}";

            var expected = new TimeSpan(days, hours, minutes, seconds);

            // Act
            var wrapper = JsonSerializer.Deserialize<TestWrapper>(duration);
            var actual = wrapper.Test;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(21, 30, 10)]
        public void
            Deserialize_ReturnsExpectedTimeSpan_WhenStringOnlyContainsTime(
                int hours,
                int minutes,
                int seconds)
        {
            // Arrange
            hours %= 24;
            minutes %= 60;
            seconds %= 60;

            var duration = $"{{ \"Test\": \"T{hours}H{minutes}M{seconds}S\" }}";

            var expected = new TimeSpan(hours, minutes, seconds);

            // Act
            var wrapper = JsonSerializer.Deserialize<TestWrapper>(duration);
            var actual = wrapper.Test;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(42)]
        public void
            Deserialize_ReturnsExpectedTimeSpan_WhenStringOnlyContainsPeriod(
                int days)
        {
            // Arrange
            var duration = $"{{ \"Test\": \"P{days}D\" }}";

            var expected = new TimeSpan(days, 0, 0, 0);

            // Act
            var wrapper = JsonSerializer.Deserialize<TestWrapper>(duration);
            var actual = wrapper.Test;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(42, 42, 42, 42)]
        public void SerializeDeserialize_RoundTrips(
            int days,
            int hours,
            int minutes,
            int seconds)
        {
            // Arrange
            var expected = new TimeSpan(days, hours, minutes, seconds);

            // Act
            using (var ms = new MemoryStream())
            {
                JsonSerializer.Serialize(ms, expected);
                var actual = JsonSerializer.Deserialize<TimeSpan>(ms);

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(42, 23, 42, 42)]
        public void DeserializeSerialize_RoundTrips(
            int days,
            int hours,
            int minutes,
            int seconds)
        {
            // Arrange
            var expected = $"\"P{days}DT{hours}H{minutes}M{seconds}S\"";
            var wrappedString = $"{{ \"Test\": {expected} }}";

            // Act
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                sw.Write(wrappedString);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                var deserialized = JsonSerializer.Deserialize<TestWrapper>(ms);
                var serialized = JsonSerializer.Serialize(deserialized.Test);
                var actual = Encoding.UTF8.GetString(serialized);

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        public class TestWrapper
        {
            public TimeSpan Test { get; set; }
        }
    }
}
