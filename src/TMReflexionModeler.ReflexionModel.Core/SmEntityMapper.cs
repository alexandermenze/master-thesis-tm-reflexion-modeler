using System.Collections.Immutable;
using TMReflexionModeler.ReflexionModel.Core.Models;

namespace TMReflexionModeler.ReflexionModel.Core;

public static class SmEntityMapper
{
    public static ImmutableArray<SmEntity> ExpandRaw(ImmutableArray<RawSmEntity> raws) =>
        [
            .. raws.SelectMany(r =>
            {
                var names = r
                    .RawDataflowNames.Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());

                var dirs = r
                    .RawDataflowMethodNames.Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                        x.Trim().StartsWith("Pull", StringComparison.OrdinalIgnoreCase)
                            ? DataflowDirection.Pull
                            : DataflowDirection.Push
                    );

                return names.Zip(
                    dirs,
                    (name, dir) =>
                        new SmEntity(
                            EntryPoint: r.EntryPoint,
                            InternalCall: r.InternalCall,
                            ExternalCall: r.ExternalCall,
                            ProcessName: r.ProcessName,
                            DataflowName: name,
                            Direction: dir
                        )
                );
            }),
        ];
}
