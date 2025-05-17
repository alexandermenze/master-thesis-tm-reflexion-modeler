namespace TMReflexionModeler.ReflexionModel.Core.Models;

public record RawSmEntity(
    string EntryPoint,
    string InternalCall,
    string ExternalCall,
    string ProcessName,
    string RawDataflowNames, // "A|B|..."
    string RawDataflowMethodNames // "Push|Pull|Push<*>|Pull<*>"
);
