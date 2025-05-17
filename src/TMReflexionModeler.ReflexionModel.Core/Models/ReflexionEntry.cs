using System.Collections.Immutable;

namespace TMReflexionModeler.ReflexionModel.Core.Models;

public record ReflexionEntry(
    string EntityType, // "Process" or "Dataflow"
    string EntityKey, // Name or Composite Key
    ReflexionCategory Category,
    ImmutableArray<string> Details,
    ImmutableArray<HlmEntity> HlmMatches,
    ImmutableArray<SmEntity> SmMatches
);
