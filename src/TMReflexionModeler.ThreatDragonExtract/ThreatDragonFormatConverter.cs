using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using TMReflexionModeler.ThreatDragonExtract.Models;

namespace TMReflexionModeler.ThreatDragonExtract;

public static class ThreatDragonFormatConverter
{
    public static void Convert(string inputPath, string outputPath, string diagramTitle = "")
    {
        var jsonText = File.ReadAllText(inputPath);
        var model =
            JsonSerializer.Deserialize<ThreatModel>(
                jsonText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? throw new InvalidOperationException("Failed to deserialize JSON");

        var diagrams = model.Detail.Diagrams;

        var diagram = string.IsNullOrEmpty(diagramTitle)
            ? diagrams.First()
            : diagrams.FirstOrDefault(d =>
                d.Title.Equals(diagramTitle, StringComparison.OrdinalIgnoreCase)
            ) ?? throw new ArgumentException($"Diagram '{diagramTitle}' not found.");

        ImmutableHashSet<string> componentTypes = ["tm.Actor", "tm.Process", "tm.Store"];

        var components = diagram
            .Cells.Where(cell => componentTypes.Contains(cell.Data.Type))
            .ToImmutableDictionary(
                cell => cell.Id,
                cell => new Component(
                    cell.Id,
                    cell.Data.Type.Split('.').Last(),
                    cell.Data.Name,
                    cell.Data.Description,
                    cell.Data.OutOfScope,
                    cell.Data.ReasonOutOfScope
                )
            );

        var flows = diagram
            .Cells.Where(cell => cell.Data.Type is "tm.Flow")
            .Where(cell => cell.Source is not null && cell.Target is not null)
            .Select(cell => new Flow(
                cell.Id,
                cell.Data.Name,
                cell.Data.OutOfScope,
                cell.Data.ReasonOutOfScope,
                cell.Data.IsBidirectional,
                cell.Source!.Cell,
                cell.Target!.Cell
            ))
            .ToImmutableArray();

        var sb = new StringBuilder();

        sb.AppendLine(
            "ComponentId,ComponentType,ComponentName,ComponentDescription,ComponentOutOfScope,ComponentReasonOutOfScope,"
                + "DataflowId,DataflowName,DataflowOutOfScope,DataflowReasonOutOfScope,"
                + "TargetComponentId,TargetComponentType,TargetComponentName,TargetComponentDescription,TargetComponentOutOfScope,TargetComponentReasonOutOfScope"
        );

        foreach (var f in flows)
        {
            var sourceComponent = components[f.SourceComponentId];
            var targetComponent = components[f.TargetComponentId];
            sb.AppendLine(Row(sourceComponent, f, targetComponent));

            if (f.IsBidirectional)
                sb.AppendLine(Row(targetComponent, f, sourceComponent));
        }

        File.WriteAllText(outputPath, sb.ToString());
    }

    private static string Row(Component c, Flow f, Component t) =>
        string.Join(
            ',',
            Escape(c.Id),
            Escape(c.Type),
            Escape(c.Name),
            Escape(c.Description),
            c.OutOfScope,
            Escape(c.ReasonOutOfScope),
            Escape(f.Id),
            Escape(f.Name),
            f.OutOfScope,
            Escape(f.ReasonOutOfScope),
            Escape(t.Id),
            Escape(t.Type),
            Escape(t.Name),
            Escape(t.Description),
            t.OutOfScope,
            Escape(t.ReasonOutOfScope)
        );

    private static string Escape(string s) =>
        s.Contains(',') || s.Contains('"') || s.Contains('\n')
            ? '"' + s.Replace("\"", "\"\"") + '"'
            : s;
}
