using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessengerAnalysis;

public class JsonDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTime));
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return DateTime.Parse(reader.GetString() ?? string.Empty);
            case JsonTokenType.Number:
                return DateTime.UnixEpoch + TimeSpan.FromMilliseconds(reader.GetDouble());
            default:
                Debug.Assert(false);
                return DateTime.MinValue;
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}