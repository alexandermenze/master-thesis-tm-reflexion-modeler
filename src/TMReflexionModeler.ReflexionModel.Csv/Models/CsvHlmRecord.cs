using CsvHelper.Configuration.Attributes;

namespace TMReflexionModeler.ReflexionModel.Csv.Models;

public class CsvHlmRecord
{
    [Name("DataflowId")]
    public required string DataflowId { get; set; }

    [Name("DataflowName")]
    public required string DataflowName { get; set; }

    [Name("DataflowOutOfScope")]
    public required bool DataflowOutOfScope { get; set; }

    [Name("ComponentId")]
    public required string ComponentId { get; set; }

    [Name("ComponentType")]
    public required string ComponentType { get; set; }

    [Name("ComponentName")]
    public required string ComponentName { get; set; }

    [Name("ComponentDescription")]
    public required string ComponentDescription { get; set; }

    [Name("ComponentOutOfScope")]
    public required bool ComponentOutOfScope { get; set; }

    [Name("TargetComponentId")]
    public required string TargetComponentId { get; set; }

    [Name("TargetComponentType")]
    public required string TargetComponentType { get; set; }

    [Name("TargetComponentName")]
    public required string TargetComponentName { get; set; }

    [Name("TargetComponentDescription")]
    public required string TargetComponentDescription { get; set; }

    [Name("TargetComponentOutOfScope")]
    public required bool TargetComponentOutOfScope { get; set; }
}
