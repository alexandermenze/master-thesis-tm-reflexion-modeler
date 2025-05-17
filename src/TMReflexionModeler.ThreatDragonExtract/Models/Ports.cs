using System.Collections.Immutable;

namespace TMReflexionModeler.ThreatDragonExtract.Models;

public record Ports(ImmutableArray<PortItem> Items);
