using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace TMReflexionModeler.SarifFormatter;

public class JsonTypeConverter<T> : DefaultTypeConverter
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public override object? ConvertFromString(
        string? text,
        IReaderRow row,
        MemberMapData memberMapData
    )
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return JsonSerializer.Deserialize<T>(text, JsonSerializerOptions);
    }
}
