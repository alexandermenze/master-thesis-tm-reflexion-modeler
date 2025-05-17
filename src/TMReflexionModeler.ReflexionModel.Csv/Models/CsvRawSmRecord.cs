using CsvHelper.Configuration.Attributes;

namespace TMReflexionModeler.ReflexionModel.Csv.Models;

public class CsvRawSmRecord
{
    [Name("entrypoint")]
    public required string EntryPoint { get; set; }

    [Name("internalcall")]
    public required string InternalCall { get; set; }

    [Name("externalcall")]
    public required string ExternalCall { get; set; }

    [Name("processName")]
    public required string ProcessName { get; set; }

    [Name("dataflowName")]
    public required string RawDataflowNames { get; set; }

    [Name("dataflowMethodName")]
    public required string RawDataflowMethodNames { get; set; }
}
