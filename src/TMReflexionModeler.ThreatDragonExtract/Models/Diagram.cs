using System.Collections.Immutable;

namespace TMReflexionModeler.ThreatDragonExtract.Models;

public record Diagram(string Title, ImmutableArray<Cell> Cells);
