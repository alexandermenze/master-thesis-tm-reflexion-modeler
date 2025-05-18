using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;

namespace TMReflexionModeler.SarifFormatter;

public static class SarifOutputFormatter
{
    private static readonly CsvConfiguration ReadConfig = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true,
        IgnoreBlankLines = true,
    };

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public static async Task<string> ConvertToSarif(string workDir, string inputPath)
    {
        using var reader = new StreamReader(inputPath);
        using var csv = new CsvReader(reader, ReadConfig);
        csv.Context.RegisterClassMap<RecordMap>();
        var records = csv.GetRecords<Record>();

        var results = new List<Dictionary<string, object>>();

        foreach (var rec in records)
        {
            var level = rec.Category.Equals("Convergence", StringComparison.OrdinalIgnoreCase)
                ? "note"
                : "warning";

            var locs = (rec.Locations ?? [])
                .Select(loc => new Dictionary<string, object>
                {
                    ["physicalLocation"] = new Dictionary<string, object>
                    {
                        ["artifactLocation"] = new Dictionary<string, object>
                        {
                            ["uri"] = loc.FilePath,
                        },
                        ["region"] = new Dictionary<string, object>
                        {
                            ["startLine"] = loc.StartLine,
                            ["startColumn"] = loc.StartColumn,
                            ["endLine"] = loc.EndLine,
                            ["endColumn"] = loc.EndColumn,
                        },
                    },
                })
                .ToImmutableArray();

            var result = new Dictionary<string, object>
            {
                ["ruleId"] = rec.Category,
                ["level"] = level,
                ["message"] = new Dictionary<string, object> { ["text"] = rec.Category },
            };

            if (locs.Length > 0)
                result["locations"] = locs;

            var props = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(rec.HlmMatches))
                props["hlmMatches"] = rec.HlmMatches;

            if (!string.IsNullOrEmpty(rec.SmMatches))
                props["smMatches"] = rec.SmMatches;

            if (props.Count > 0)
                result["properties"] = props;

            results.Add(result);
        }

        var outputPath = Path.Combine(workDir, "reflexion-model.sarif");
        return await WriteResults(outputPath, results);
    }

    private static async Task<string> WriteResults(
        string outputPath,
        IReadOnlyCollection<IDictionary<string, object>> results
    )
    {
        var sarifLog = new Dictionary<string, object>
        {
            ["$schema"] = "https://schemastore.azurewebsites.net/schemas/json/sarif-2.1.0.json",
            ["version"] = "2.1.0",
            ["runs"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["tool"] = new Dictionary<string, object>
                    {
                        ["driver"] = new Dictionary<string, object>
                        {
                            ["name"] = "tm-reflexion-modeler",
                            ["informationUri"] = "https://alexandermenze.de/tm-reflexion-modeler",
                            ["rules"] = new[]
                            {
                                new Dictionary<string, object>
                                {
                                    ["id"] = "Divergence",
                                    ["shortDescription"] = new Dictionary<string, object>
                                    {
                                        ["text"] =
                                            "Divergence between HLM and SM, which is a component from HLM without mapping.",
                                    },
                                },
                                new Dictionary<string, object>
                                {
                                    ["id"] = "Absence",
                                    ["shortDescription"] = new Dictionary<string, object>
                                    {
                                        ["text"] = "Absence of component in SM",
                                    },
                                },
                                new Dictionary<string, object>
                                {
                                    ["id"] = "Convergence",
                                    ["shortDescription"] = new Dictionary<string, object>
                                    {
                                        ["text"] = "Convergence between HLM and SM",
                                    },
                                },
                            },
                        },
                    },
                    ["results"] = results,
                },
            },
        };

        await File.WriteAllTextAsync(
            outputPath,
            JsonSerializer.Serialize(sarifLog, JsonSerializerOptions)
        );

        Console.WriteLine($"SARIF report generated at {outputPath}");

        return outputPath;
    }
}
