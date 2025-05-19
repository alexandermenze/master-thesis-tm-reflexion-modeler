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

    public static async Task<string> ConvertToSarif(
        string workDir,
        string inputPath,
        string pwd,
        string hlmPath
    )
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

            // Add physical locations
            var locs = (rec.Locations ?? [])
                .Distinct()
                .Select(loc =>
                    CreateLocationEntry(
                        pwd,
                        loc.FilePath,
                        loc.StartLine,
                        loc.StartColumn,
                        loc.EndLine,
                        loc.EndColumn,
                        MakeMessage(rec)
                    )
                )
                .ToList();

            // Ensure has at least one location
            if (locs.Count == 0)
                locs.Add(
                    new Dictionary<string, object>
                    {
                        ["physicalLocation"] = new Dictionary<string, object>
                        {
                            ["artifactLocation"] = new Dictionary<string, object>
                            {
                                ["uri"] = MakeRelative(hlmPath, pwd),
                            },
                        },
                    }
                );

            // Add logical locations from HLM and SM matches
            var logicalLocations = SplitMatches(rec.HlmMatches)
                .Concat(SplitMatches(rec.SmMatches))
                .Distinct()
                .Select(CreateLogicalLocationEntry)
                .ToImmutableArray();

            if (logicalLocations.IsEmpty is false)
                locs.Add(
                    new Dictionary<string, object> { { "logicalLocations", logicalLocations } }
                );

            // Compose SARIF result
            var result = new Dictionary<string, object>
            {
                ["ruleId"] = rec.Category,
                ["level"] = level,
                ["message"] = new Dictionary<string, object> { ["text"] = MakeMessage(rec) },
                ["locations"] = locs,
            };

            results.Add(result);
        }

        var outputPath = Path.Combine(workDir, "reflexion-model.sarif");
        return await WriteResults(outputPath, results);
    }

    private static Dictionary<string, object> CreateLocationEntry(
        string pwd,
        string uri,
        int startLine,
        int startColumn,
        int endLine,
        int endColumn,
        string message
    ) =>
        new()
        {
            ["message"] = new Dictionary<string, object> { ["text"] = message },
            ["physicalLocation"] = new Dictionary<string, object>
            {
                ["artifactLocation"] = new Dictionary<string, object>
                {
                    ["uri"] = MakeRelative(uri, pwd),
                },
                ["region"] = new Dictionary<string, object>
                {
                    ["startLine"] = startLine,
                    ["startColumn"] = startColumn,
                    ["endLine"] = endLine,
                    ["endColumn"] = endColumn,
                },
            },
        };

    private static Dictionary<string, object> CreateLogicalLocationEntry(
        string fullyQualifiedName
    ) => new() { ["fullyQualifiedName"] = fullyQualifiedName };

    private static IEnumerable<string> SplitMatches(string matches)
    {
        if (string.IsNullOrWhiteSpace(matches))
            yield break;

        foreach (var part in matches.Split('|', StringSplitOptions.RemoveEmptyEntries))
            yield return part.Trim();
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
                                        ["text"] = "Divergence between HLM and SM",
                                    },
                                },
                                new Dictionary<string, object>
                                {
                                    ["id"] = "Absence",
                                    ["shortDescription"] = new Dictionary<string, object>
                                    {
                                        ["text"] = "Absence in SM",
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

    private static string MakeMessage(Record rec)
    {
        return $"""
            {rec.Category} for {rec.EntityType} {rec.EntityKey} detected.

            High-Level Model Components are: {string.Join(
                Environment.NewLine,
                rec.HlmMatches.Split('|', StringSplitOptions.RemoveEmptyEntries)
            )}.

            Source Model Components are: {string.Join(
                Environment.NewLine,
                rec.SmMatches.Split('|', StringSplitOptions.RemoveEmptyEntries)
            )}
            """;
    }

    private static string MakeRelative(string child, string root)
    {
        child = Path.GetFullPath(child);
        root = Path.GetFullPath(root);

        return Path.GetRelativePath(root, child);
    }
}
