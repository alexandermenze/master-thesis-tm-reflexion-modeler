namespace TMReflexionModeler.ReflexionModel.Core.Models;

public record SmEntity(
    string EntryPoint,
    string InternalCall,
    string ExternalCall,
    string ProcessName,
    string DataflowName,
    DataflowDirection Direction,
    Location EntryPointLocation,
    Location InternalCallLocation
);
