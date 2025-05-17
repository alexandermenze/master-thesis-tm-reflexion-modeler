namespace TMReflexionModeler.ThreatDragonExtract.Models;

public record Component(
    string Id,
    string Type,
    string Name,
    string Description,
    bool OutOfScope,
    string ReasonOutOfScope
);
