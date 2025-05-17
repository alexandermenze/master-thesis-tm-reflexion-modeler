namespace TMReflexionModeler.ThreatDragonExtract.Models;

public record Flow(
    string Id,
    string Name,
    bool OutOfScope,
    string ReasonOutOfScope,
    bool IsBidirectional,
    string SourceComponentId,
    string TargetComponentId
);
