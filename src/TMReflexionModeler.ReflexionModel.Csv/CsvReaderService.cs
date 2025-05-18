using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using TMReflexionModeler.ReflexionModel.Core.Models;
using TMReflexionModeler.ReflexionModel.Csv.Models;

namespace TMReflexionModeler.ReflexionModel.Csv;

public class CsvReaderService : ICsvReaderService
{
    private readonly CsvConfiguration _readConfig = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true,
        IgnoreBlankLines = true,
    };

    private readonly CsvConfiguration _writeConfig = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true,
    };

    public ImmutableArray<HlmEntity> LoadHlm(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, _readConfig);
        var records = csv.GetRecords<CsvHlmRecord>()
            .Select(r => new HlmEntity(
                new Dataflow(
                    r.DataflowId.Trim(),
                    r.DataflowName.Trim(),
                    r.DataflowOutOfScope,
                    new Component(
                        r.ComponentId.Trim(),
                        r.ComponentType.Trim(),
                        r.ComponentName.Trim(),
                        r.ComponentDescription.Trim(),
                        r.ComponentOutOfScope
                    ),
                    new Component(
                        r.TargetComponentId.Trim(),
                        r.TargetComponentType.Trim(),
                        r.TargetComponentName.Trim(),
                        r.TargetComponentDescription.Trim(),
                        r.TargetComponentOutOfScope
                    )
                )
            ))
            .ToImmutableArray();
        return records;
    }

    public ImmutableArray<SmEntity> LoadSm(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, _readConfig);
        var rawRecords = csv.GetRecords<CsvRawSmRecord>().ToList();

        var smList = rawRecords
            .SelectMany(r =>
            {
                var entryPointLocation = new Location(
                    r.ProcessFilePath,
                    r.ProcessStartLine,
                    r.ProcessStartColumn,
                    r.ProcessEndLine,
                    r.ProcessEndColumn
                );

                var internalCallLocation = new Location(
                    r.InternalCallFilePath,
                    r.InternalCallStartLine,
                    r.InternalCallStartColumn,
                    r.InternalCallEndLine,
                    r.InternalCallEndColumn
                );

                if (string.IsNullOrWhiteSpace(r.RawDataflowNames))
                    return
                    [
                        new SmEntity(
                            EntryPoint: r.EntryPoint.Trim(),
                            InternalCall: r.InternalCall.Trim(),
                            ExternalCall: r.ExternalCall.Trim(),
                            ProcessName: r.ProcessName.Trim(),
                            DataflowName: "",
                            Direction: DataflowDirection.Unknown,
                            entryPointLocation,
                            internalCallLocation
                        ),
                    ];

                var names = MultiValueConverter.SplitAndNormalize(r.RawDataflowNames);
                var directions = MultiValueConverter
                    .SplitAndNormalize(r.RawDataflowMethodNames)
                    .Select(m =>
                        m.StartsWith("Pull", StringComparison.OrdinalIgnoreCase)
                            ? DataflowDirection.Pull
                            : DataflowDirection.Push
                    );

                return names.Zip(
                    directions,
                    (df, direction) =>
                        new SmEntity(
                            EntryPoint: r.EntryPoint.Trim(),
                            InternalCall: r.InternalCall.Trim(),
                            ExternalCall: r.ExternalCall.Trim(),
                            ProcessName: r.ProcessName.Trim(),
                            DataflowName: df,
                            Direction: direction,
                            entryPointLocation,
                            internalCallLocation
                        )
                );
            })
            .ToImmutableArray();

        return smList;
    }

    public void WriteReflexion(string path, ImmutableArray<ReflexionEntry> entries)
    {
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, _writeConfig);

        csv.WriteField("EntityType");
        csv.WriteField("EntityKey");
        csv.WriteField("Category");
        csv.WriteField("Details");
        csv.WriteField("HlmMatches");
        csv.WriteField("SmMatches");
        csv.WriteField("Locations");
        csv.NextRecord();

        foreach (var e in entries)
        {
            var isProcess = e.EntityType.Equals("Process", StringComparison.OrdinalIgnoreCase);

            var details = string.Join("|", e.Details);
            var hlmMatches = string.Join(
                "|",
                e.HlmMatches.Select(h =>
                    $"{h.Flow.Source.Type}:{h.Flow.Source.Name}:{h.Flow.Name}:{h.Flow.Target.Name}"
                )
            );
            var smMatches = string.Join(
                "|",
                e.SmMatches.Select(s =>
                    $"{s.ProcessName}:{s.EntryPoint}:{s.ExternalCall}:{s.DataflowName}:{s.Direction}"
                )
            );

            csv.WriteField(e.EntityType);
            csv.WriteField(e.EntityKey);
            csv.WriteField(e.Category.ToString());
            csv.WriteField(details);
            csv.WriteField(hlmMatches);
            csv.WriteField(smMatches);
            csv.WriteField(
                JsonSerializer.Serialize(
                    e.SmMatches.Select(s =>
                            isProcess ? s.EntryPointLocation : s.InternalCallLocation
                        )
                        .Distinct()
                        .ToImmutableArray()
                )
            );
            csv.NextRecord();
        }
        writer.Flush();
    }
}
