namespace TMReflexionModeler.ThreatDragonExtract.Models;

public record Cell(string Id, CellData Data, Ports? Ports, Link? Source, Link? Target);
