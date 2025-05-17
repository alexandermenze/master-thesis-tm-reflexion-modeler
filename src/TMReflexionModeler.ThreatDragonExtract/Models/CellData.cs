namespace TMReflexionModeler.ThreatDragonExtract.Models;

public record CellData(
    string Type,
    string Name,
    string Description,
    bool OutOfScope,
    string ReasonOutOfScope,
    bool IsBidirectional
);
