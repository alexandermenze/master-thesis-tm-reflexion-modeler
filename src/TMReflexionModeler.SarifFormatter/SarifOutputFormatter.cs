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
                .Select(loc =>
                    CreateLocationEntry(
                        loc.FilePath,
                        loc.StartLine,
                        loc.StartColumn,
                        loc.EndLine,
                        loc.EndColumn
                    )
                )
                .ToList();

            // If Absence has no physical location, add a placeholder or relatedLocation
            if (
                rec.Category.Equals("Absence", StringComparison.OrdinalIgnoreCase)
                && locs.Count is 0
            )
            {
                locs.Add(
                    new Dictionary<string, object>
                    {
                        ["physicalLocation"] = new Dictionary<string, object>
                        {
                            ["artifactLocation"] = new Dictionary<string, object>
                            {
                                ["uri"] = rec.HlmMatches.Split(
                                    '|',
                                    StringSplitOptions.RemoveEmptyEntries
                                )[0],
                            },
                        },
                    }
                );
            }

            // Split hlmMatches and smMatches into additional relatedLocations
            var related = SplitMatches(rec.HlmMatches)
                .Concat(SplitMatches(rec.SmMatches))
                .Select(match => new Dictionary<string, object>
                {
                    ["physicalLocation"] = new Dictionary<string, object>
                    {
                        ["artifactLocation"] = new Dictionary<string, object> { ["uri"] = match },
                    },
                })
                .ToList();

            var result = new Dictionary<string, object>
            {
                ["ruleId"] = rec.Category,
                ["level"] = level,
                ["message"] = new Dictionary<string, object> { ["text"] = rec.Category },
                ["locations"] = locs,
            };

            if (related.Count > 0)
                result["relatedLocations"] = related;

            results.Add(result);
        }

        var outputPath = Path.Combine(workDir, "reflexion-model.sarif");
        return await WriteResults(outputPath, results);
    }

    private static Dictionary<string, object> CreateLocationEntry(
        string uri,
        int startLine,
        int startColumn,
        int endLine,
        int endColumn
    )
    {
        return new Dictionary<string, object>
        {
            ["physicalLocation"] = new Dictionary<string, object>
            {
                ["artifactLocation"] = new Dictionary<string, object> { ["uri"] = uri },
                ["region"] = new Dictionary<string, object>
                {
                    ["startLine"] = startLine,
                    ["startColumn"] = startColumn,
                    ["endLine"] = endLine,
                    ["endColumn"] = endColumn,
                },
            },
        };
    }

    private static IEnumerable<string> SplitMatches(string matches)
    {
        if (string.IsNullOrWhiteSpace(matches))
            yield break;

        foreach (var part in matches.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            yield return part.Trim();
        }
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
