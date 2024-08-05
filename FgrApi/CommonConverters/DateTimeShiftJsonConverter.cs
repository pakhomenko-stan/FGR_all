using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonConverters
{
    public class DateTimeShiftJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var inputValue = reader.GetString();
            var date = DateTime.TryParseExact(inputValue, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime dt) ? dt : DateTime.Now;
            date = date.AddHours(12);
            var result = new DateTime(year: date.Year, month: date.Month, day: date.Day, hour: 12, minute: 0, second: 0, DateTimeKind.Utc);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
