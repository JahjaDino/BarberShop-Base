using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BarberShop.API.Serialization;

public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] AcceptedFormats =
    [
        "HH:mm",
        "H:mm",
        "HH:mm:ss",
        "H:mm:ss",
        "HH:mm:ss.FFFFFFF",
        "H:mm:ss.FFFFFFF",
        "hh:mm tt",
        "h:mm tt"
    ];

    public override TimeOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("TimeOnly value must be a string in HH:mm format.");
        }

        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("TimeOnly value is required.");
        }

        if (TimeOnly.TryParseExact(
                value.Trim(),
                AcceptedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedTime))
        {
            return parsedTime;
        }

        throw new JsonException("TimeOnly value must be in HH:mm format.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        TimeOnly value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm", CultureInfo.InvariantCulture));
    }
}
