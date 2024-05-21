using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace ProductService.IntegrationTest;

public class TimestampConverter : JsonConverter<Timestamp>
{
    public override Timestamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var dateTime))
            return Timestamp.FromDateTime(dateTime);

        throw new JsonException("Unable to parse Timestamp.");
    }

    public override void Write(Utf8JsonWriter writer, Timestamp value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToDateTimeOffset().ToString("o"));
    }
}