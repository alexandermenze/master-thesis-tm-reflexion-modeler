using System.Collections.Immutable;
using CsvHelper.Configuration;
using TMReflexionModeler.ReflexionModel.Core.Models;

namespace TMReflexionModeler.SarifFormatter;

public sealed class RecordMap : ClassMap<Record>
{
    public RecordMap()
    {
        Map(m => m.EntityType).Name("EntityType");
        Map(m => m.EntityKey).Name("EntityKey");
        Map(m => m.Category).Name("Category");
        Map(m => m.Details).Name("Details");
        Map(m => m.HlmMatches).Name("HlmMatches");
        Map(m => m.SmMatches).Name("SmMatches");
        Map(m => m.Locations)
            .Name("Locations")
            .TypeConverter(new JsonTypeConverter<ImmutableArray<Location>>());
    }
}
