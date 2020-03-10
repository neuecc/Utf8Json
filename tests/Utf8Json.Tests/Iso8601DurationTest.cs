using System;
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

        public class TestWrapper
        {
            public TimeSpan Test { get; set; }
        }
    }
}
