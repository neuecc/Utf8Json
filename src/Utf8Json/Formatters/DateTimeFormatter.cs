using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
    public sealed class DateTimeFormatter : IJsonFormatter<DateTime>
    {
        readonly string formatString;

        public DateTimeFormatter()
        {
            this.formatString = null;
        }

        public DateTimeFormatter(string formatString)
        {
            this.formatString = formatString;
        }

        public void Serialize(ref JsonWriter writer, DateTime value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteString(value.ToString(formatString));
        }

        public DateTime Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var str = reader.ReadString();
            if (formatString == null)
            {
                return DateTime.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                return DateTime.ParseExact(str, formatString, CultureInfo.InvariantCulture);
            }
        }
    }

    public sealed class ISO8601DateTimeFormatter : IJsonFormatter<DateTime>
    {
        public static readonly IJsonFormatter<DateTime> Default = new ISO8601DateTimeFormatter();

        public void Serialize(ref JsonWriter writer, DateTime value, IJsonFormatterResolver formatterResolver)
        {
            var year = value.Year;
            var month = value.Month;
            var day = value.Day;
            var hour = value.Hour;
            var minute = value.Minute;
            var second = value.Second;
            var nanosecond = value.Ticks % TimeSpan.TicksPerSecond;

            const int baseLength = 19 + 2; // {YEAR}-{MONTH}-{DAY}T{Hour}:{Minute}:{Second} + quotation
            const int nanosecLength = 8; // .{nanoseconds}

            switch (value.Kind)
            {
                case DateTimeKind.Local:
                    // +{Hour}:{Minute}
                    writer.EnsureCapacity(baseLength + ((nanosecond == 0) ? 0 : nanosecLength) + 6);
                    break;
                case DateTimeKind.Utc:
                    // Z
                    writer.EnsureCapacity(baseLength + ((nanosecond == 0) ? 0 : nanosecLength) + 1);
                    break;
                case DateTimeKind.Unspecified:
                default:
                    writer.EnsureCapacity(baseLength + ((nanosecond == 0) ? 0 : nanosecLength));
                    break;
            }

            writer.WriteRawUnsafe((byte)'\"');

            if (year < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
                writer.WriteRawUnsafe((byte)'0');
                writer.WriteRawUnsafe((byte)'0');
            }
            else if (year < 100)
            {
                writer.WriteRawUnsafe((byte)'0');
                writer.WriteRawUnsafe((byte)'0');
            }
            else if (year < 1000)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(year);
            writer.WriteRawUnsafe((byte)'-');

            if (month < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(month);
            writer.WriteRawUnsafe((byte)'-');

            if (day < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(day);

            writer.WriteRawUnsafe((byte)'T');

            if (hour < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(hour);
            writer.WriteRawUnsafe((byte)':');

            if (minute < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(minute);
            writer.WriteRawUnsafe((byte)':');

            if (second < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(second);

            if (nanosecond != 0)
            {
                writer.WriteRawUnsafe((byte)'.');
                writer.WriteInt64(nanosecond);
            }

            switch (value.Kind)
            {
                case DateTimeKind.Local:
                    var localOffset = TimeZoneInfo.Local.BaseUtcOffset;
                    var h = localOffset.Hours;
                    var m = localOffset.Minutes;
                    writer.WriteRawUnsafe((localOffset < TimeSpan.Zero) ? (byte)'-' : (byte)'+');
                    if (h < 10)
                    {
                        writer.WriteRawUnsafe((byte)'0');
                    }
                    writer.WriteInt32(h);
                    writer.WriteRawUnsafe((byte)':');
                    if (m < 10)
                    {
                        writer.WriteRawUnsafe((byte)'0');
                    }
                    writer.WriteInt32(m);
                    break;
                case DateTimeKind.Utc:
                    writer.WriteRawUnsafe((byte)'Z');
                    break;
                case DateTimeKind.Unspecified:
                default:
                    break;
            }

            writer.WriteRawUnsafe((byte)'\"');
        }

        public DateTime Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var str = reader.ReadStringSegmentUnsafe();
            var array = str.Array;
            var i = str.Offset;

            // range-first section requires 19
            if (array.Length < 19) goto ERROR;

            var year = (array[i++] - (byte)'0') * 1000 + (array[i++] - (byte)'0') * 100 + (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)'-') goto ERROR;
            var month = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)'-') goto ERROR;
            var day = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

            if (array[i++] != (byte)'T') goto ERROR;

            var hour = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var minute = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var second = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

            var readCount = 19;
            int millisecond = 0;
            if (readCount < str.Count && array[i] == '.')
            {
                i++;
                readCount++;
                int? milli1 = null;
                int? milli2 = null;
                int? milli3 = null;

                if (readCount < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli1 = array[i];
                i++;
                readCount++;

                if (readCount < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli2 = array[i];
                i++;
                readCount++;

                if (readCount < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli3 = array[i];
                i++;
                readCount++;

                // others, lack of precision
                while (readCount < str.Count && NumberConverter.IsNumber(array[i]))
                {
                    i++;
                    readCount++;
                }

                CREATE_MICROSEC:
                if (milli3 != null)
                {
                    millisecond = (milli1.Value - (byte)'0') * 100 + (milli2.Value - (byte)'0') * 10 + (milli3.Value - (byte)'0');
                }
                else if (milli2 != null)
                {
                    millisecond = (milli1.Value - (byte)'0') * 10 + (milli2.Value - (byte)'0');
                }
                else if (milli1 != null)
                {
                    millisecond = (milli1.Value - (byte)'0');
                }
            }

            var kind = DateTimeKind.Unspecified;
            if (readCount < str.Count && array[i] == 'Z')
            {
                kind = DateTimeKind.Utc;
            }
            else if (readCount < str.Count && array[i] == '-' || array[i] == '+')
            {
                if (!(readCount + 5 < str.Count)) goto ERROR;

                kind = DateTimeKind.Local;
                var minus = array[i++] == '-';

                var h = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                i++;
                var m = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

                var offset = new TimeSpan(h, m, 0);
                if (minus) offset = offset.Negate();

                return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc).Subtract(offset).ToLocalTime();
            }

            return new DateTime(year, month, day, hour, minute, second, millisecond, kind);

            ERROR:
            throw new InvalidOperationException("invalid datetime format. value:" + StringEncoding.UTF8.GetString(str.Array, str.Offset, str.Count));
        }
    }

    public sealed class UnixTimestampDateTimeFormatter : IJsonFormatter<DateTime>
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public void Serialize(ref JsonWriter writer, DateTime value, IJsonFormatterResolver formatterResolver)
        {
            var ticks = (long)(value.ToUniversalTime() - UnixEpoch).TotalSeconds;
            writer.WriteQuotation();
            writer.WriteInt64(ticks);
            writer.WriteQuotation();
        }

        public DateTime Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var str = reader.ReadStringSegmentUnsafe();
            int readCount;
            var ticks = NumberConverter.ReadUInt64(str.Array, str.Offset, out readCount);

            return UnixEpoch.AddSeconds(ticks);
        }
    }

    public sealed class DateTimeOffsetFormatter : IJsonFormatter<DateTimeOffset>
    {
        readonly string formatString;

        public DateTimeOffsetFormatter()
        {
            this.formatString = null;
        }

        public DateTimeOffsetFormatter(string formatString)
        {
            this.formatString = formatString;
        }

        public void Serialize(ref JsonWriter writer, DateTimeOffset value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteString(value.ToString(formatString));
        }

        public DateTimeOffset Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var str = reader.ReadString();
            if (formatString == null)
            {
                return DateTimeOffset.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                return DateTimeOffset.ParseExact(str, formatString, CultureInfo.InvariantCulture);
            }
        }
    }

    public sealed class ISO8601DateTimeOffsetFormatter : IJsonFormatter<DateTimeOffset>
    {
        public static readonly IJsonFormatter<DateTimeOffset> Default = new ISO8601DateTimeOffsetFormatter();

        public void Serialize(ref JsonWriter writer, DateTimeOffset value, IJsonFormatterResolver formatterResolver)
        {
            var year = value.Year;
            var month = value.Month;
            var day = value.Day;
            var hour = value.Hour;
            var minute = value.Minute;
            var second = value.Second;
            var nanosecond = value.Ticks % TimeSpan.TicksPerSecond;

            const int baseLength = 19 + 2; // {YEAR}-{MONTH}-{DAY}T{Hour}:{Minute}:{Second} + quotation
            const int nanosecLength = 8; // .{nanoseconds}

            // +{Hour}:{Minute}
            writer.EnsureCapacity(baseLength + ((nanosecond == 0) ? 0 : nanosecLength) + 6);

            writer.WriteRawUnsafe((byte)'\"');

            if (year < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
                writer.WriteRawUnsafe((byte)'0');
                writer.WriteRawUnsafe((byte)'0');
            }
            else if (year < 100)
            {
                writer.WriteRawUnsafe((byte)'0');
                writer.WriteRawUnsafe((byte)'0');
            }
            else if (year < 1000)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(year);
            writer.WriteRawUnsafe((byte)'-');

            if (month < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(month);
            writer.WriteRawUnsafe((byte)'-');

            if (day < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(day);

            writer.WriteRawUnsafe((byte)'T');

            if (hour < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(hour);
            writer.WriteRawUnsafe((byte)':');

            if (minute < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(minute);
            writer.WriteRawUnsafe((byte)':');

            if (second < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(second);

            if (nanosecond != 0)
            {
                writer.WriteRawUnsafe((byte)'.');
                writer.WriteInt64(nanosecond);
            }

            var localOffset = value.Offset;
            var h = localOffset.Hours;
            var m = localOffset.Minutes;
            writer.WriteRawUnsafe((localOffset < TimeSpan.Zero) ? (byte)'-' : (byte)'+');
            if (h < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(h);
            writer.WriteRawUnsafe((byte)':');
            if (m < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(m);

            writer.WriteRawUnsafe((byte)'\"');
        }

        public DateTimeOffset Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var str = reader.ReadStringSegmentUnsafe();
            var array = str.Array;
            var i = str.Offset;

            // range-first section requires 19
            if (array.Length < 19) goto ERROR;

            var year = (array[i++] - (byte)'0') * 1000 + (array[i++] - (byte)'0') * 100 + (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)'-') goto ERROR;
            var month = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)'-') goto ERROR;
            var day = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

            if (array[i++] != (byte)'T') goto ERROR;

            var hour = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var minute = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var second = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

            var readCount = 19;
            int millisecond = 0;
            if (readCount < str.Count && array[i] == '.')
            {
                i++;
                readCount++;
                int? milli1 = null;
                int? milli2 = null;
                int? milli3 = null;

                if (readCount < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli1 = array[i];
                i++;
                readCount++;

                if (readCount < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli2 = array[i];
                i++;
                readCount++;

                if (readCount < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli3 = array[i];
                i++;
                readCount++;

                // others, lack of precision
                while (readCount < str.Count && NumberConverter.IsNumber(array[i]))
                {
                    i++;
                    readCount++;
                }

                CREATE_MICROSEC:
                if (milli3 != null)
                {
                    millisecond = (milli1.Value - (byte)'0') * 100 + (milli2.Value - (byte)'0') * 10 + (milli3.Value - (byte)'0');
                }
                else if (milli2 != null)
                {
                    millisecond = (milli1.Value - (byte)'0') * 10 + (milli2.Value - (byte)'0');
                }
                else if (milli1 != null)
                {
                    millisecond = (milli1.Value - (byte)'0');
                }
            }

            if (readCount < str.Count && array[i] == '-' || array[i] == '+')
            {
                if (!(readCount + 5 < str.Count)) goto ERROR;

                var minus = array[i++] == '-';

                var h = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                i++;
                var m = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

                var offset = new TimeSpan(h, m, 0);
                if (minus) offset = offset.Negate();

                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
            }

            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, TimeSpan.Zero);

            ERROR:
            throw new InvalidOperationException("invalid datetime format. value:" + StringEncoding.UTF8.GetString(str.Array, str.Offset, str.Count));
        }
    }

    public sealed class TimeSpanFormatter : IJsonFormatter<TimeSpan>
    {
        readonly string formatString;

        public TimeSpanFormatter()
        {
            this.formatString = null;
        }

        public TimeSpanFormatter(string formatString)
        {
            this.formatString = formatString;
        }

        public void Serialize(ref JsonWriter writer, TimeSpan value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteString(value.ToString(formatString));
        }

        public TimeSpan Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var str = reader.ReadString();
            if (formatString == null)
            {
                return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                return TimeSpan.ParseExact(str, formatString, CultureInfo.InvariantCulture);
            }
        }
    }

    public sealed class ISO8601TimeSpanFormatter : IJsonFormatter<TimeSpan>
    {
        public static readonly IJsonFormatter<TimeSpan> Default = new ISO8601TimeSpanFormatter();

        public void Serialize(ref JsonWriter writer, TimeSpan value, IJsonFormatterResolver formatterResolver)
        {
            var day = value.Days;
            var hour = value.Hours;
            var minute = value.Minutes;
            var second = value.Seconds;
            var nanosecond = value.Ticks % TimeSpan.TicksPerSecond;

            const int maxDayLength = 8 + 1; // {Day}.
            const int baseLength = 8 + 2; // {Hour}:{Minute}:{Second} + quotation
            const int nanosecLength = 8; // .{nanoseconds}

            writer.EnsureCapacity(baseLength + ((maxDayLength == 0) ? 0 : maxDayLength) + ((nanosecond == 0) ? 0 : nanosecLength) + 6);

            writer.WriteRawUnsafe((byte)'\"');

            if (day != 0)
            {
                writer.WriteInt32(day);
                writer.WriteRawUnsafe((byte)'.');
            }

            if (hour < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(hour);
            writer.WriteRawUnsafe((byte)':');

            if (minute < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(minute);
            writer.WriteRawUnsafe((byte)':');

            if (second < 10)
            {
                writer.WriteRawUnsafe((byte)'0');
            }
            writer.WriteInt32(second);

            if (nanosecond != 0)
            {
                writer.WriteRawUnsafe((byte)'.');
                writer.WriteInt64(nanosecond);
            }

            writer.WriteRawUnsafe((byte)'\"');
        }

        public TimeSpan Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var str = reader.ReadStringSegmentUnsafe();
            var array = str.Array;
            var i = str.Offset;

            // check day exists
            bool hasDay = false;
            {
                bool foundDot = false;
                bool foundColon = false;
                for (int j = i; j < str.Count; j++)
                {
                    if (array[j] == '.')
                    {
                        if (foundColon)
                        {
                            break;
                        }
                        foundDot = true;
                    }
                    else if (array[j] == ':')
                    {
                        if (foundDot)
                        {
                            hasDay = true;
                        }
                        foundColon = true;
                    }
                }
            }

            var day = 0;
            if (hasDay)
            {
                var poolArray = BufferPool.Default.Rent();
                try
                {
                    for (; array[i] != '.'; i++)
                    {
                        poolArray[day++] = array[i];
                    }
                    day = new JsonReader(poolArray).ReadInt32();
                    i++; // skip '.'
                }
                finally
                {
                    BufferPool.Default.Return(poolArray);
                }
            }

            var hour = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var minute = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var second = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

            int millisecond = 0;
            if (i < str.Count && array[i] == '.')
            {
                i++;
                int? milli1 = null;
                int? milli2 = null;
                int? milli3 = null;

                if (i < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli1 = array[i];
                i++;

                if (i < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli2 = array[i];
                i++;

                if (i < str.Count && !NumberConverter.IsNumber(array[i])) goto CREATE_MICROSEC;
                milli3 = array[i];
                i++;

                // others, lack of precision
                while (i < str.Count && NumberConverter.IsNumber(array[i]))
                {
                    i++;
                }

                CREATE_MICROSEC:
                if (milli3 != null)
                {
                    millisecond = (milli1.Value - (byte)'0') * 100 + (milli2.Value - (byte)'0') * 10 + (milli3.Value - (byte)'0');
                }
                else if (milli2 != null)
                {
                    millisecond = (milli1.Value - (byte)'0') * 10 + (milli2.Value - (byte)'0');
                }
                else if (milli1 != null)
                {
                    millisecond = (milli1.Value - (byte)'0');
                }
            }

            return new TimeSpan(day, hour, minute, second, millisecond);

            ERROR:
            throw new InvalidOperationException("invalid datetime format. value:" + StringEncoding.UTF8.GetString(str.Array, str.Offset, str.Count));
        }
    }
}
