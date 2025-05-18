using System.Collections.Immutable;
using TMReflexionModeler.ReflexionModel.Core.Models;

namespace TMReflexionModeler.SarifFormatter;

public class Record
{
    public required string EntityType { get; set; }
    public required string EntityKey { get; set; }
    public required string Category { get; set; }
    public required string Details { get; set; }
    public required string HlmMatches { get; set; }
    public required string SmMatches { get; set; }
    public ImmutableArray<Location>? Locations { get; set; }
}
