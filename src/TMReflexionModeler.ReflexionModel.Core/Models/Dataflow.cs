namespace TMReflexionModeler.ReflexionModel.Core.Models;

public record Dataflow(string Id, string Name, bool OutOfScope, Component Source, Component Target);
