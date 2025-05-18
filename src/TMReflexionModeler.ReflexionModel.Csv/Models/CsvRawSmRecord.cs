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

    [Name("processname")]
    public required string ProcessName { get; set; }

    [Name("dataflowname")]
    public required string RawDataflowNames { get; set; }

    [Name("dataflowmethodname")]
    public required string RawDataflowMethodNames { get; set; }

    [Name("internalcallfilepath")]
    public required string InternalCallFilePath { get; set; }

    [Name("internalcallstartline")]
    public required int InternalCallStartLine { get; set; }

    [Name("internalcallstartcolumn")]
    public required int InternalCallStartColumn { get; set; }

    [Name("internalcallendline")]
    public required int InternalCallEndLine { get; set; }

    [Name("internalcallendcolumn")]
    public required int InternalCallEndColumn { get; set; }

    [Name("processfilepath")]
    public required string ProcessFilePath { get; set; }

    [Name("processstartline")]
    public required int ProcessStartLine { get; set; }

    [Name("processstartcolumn")]
    public required int ProcessStartColumn { get; set; }

    [Name("processendline")]
    public required int ProcessEndLine { get; set; }

    [Name("processendcolumn")]
    public required int ProcessEndColumn { get; set; }
}
