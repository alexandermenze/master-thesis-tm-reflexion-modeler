using System.Collections.Immutable;
using TMReflexionModeler.ReflexionModel.Core.Models;

namespace TMReflexionModeler.ReflexionModel.Core;

public static class ReflexionProcessMapper
{
    public static IEnumerable<ReflexionEntry> MapProcesses(
        ImmutableArray<HlmEntity> hlm,
        ImmutableArray<SmEntity> sm
    )
    {
        var hlmProcessNames = hlm.Select(h => h.Flow.Source)
            .Where(c => c.Type.Equals("Process", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Name.Trim())
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        var hlmProcessTargetNames = hlm.Select(h => h.Flow.Target)
            .Where(c => c.Type.Equals("Process", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Name.Trim())
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        var smProcessNames = sm.Select(s => s.ProcessName.Trim())
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        // Convergence: SLM-Process found in HLM as source or target
        foreach (
            var processName in smProcessNames.Where(p =>
                hlmProcessNames.Contains(p) || hlmProcessTargetNames.Contains(p)
            )
        )
        {
            var hlmMatches = hlm.Where(h =>
                    h.Flow.Source.Name.Equals(processName, StringComparison.OrdinalIgnoreCase)
                    || h.Flow.Target.Name.Equals(processName, StringComparison.OrdinalIgnoreCase)
                )
                .ToImmutableArray();
            var smMatches = sm.Where(s =>
                    s.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)
                )
                .ToImmutableArray();

            yield return new ReflexionEntry(
                EntityType: "Process",
                EntityKey: processName,
                Category: ReflexionCategory.Convergence,
                Details: [],
                HlmMatches: hlmMatches,
                SmMatches: smMatches
            );
        }

        // Absence: HLM process not found in SLM
        foreach (
            var processName in hlmProcessNames
                .Union(hlmProcessTargetNames)
                .Where(p => !smProcessNames.Contains(p))
        )
        {
            var hlmMatches = hlm.Where(h =>
                    h.Flow.Source.Name.Equals(processName, StringComparison.OrdinalIgnoreCase)
                    || h.Flow.Target.Name.Equals(processName, StringComparison.OrdinalIgnoreCase)
                )
                .ToImmutableArray();

            yield return new ReflexionEntry(
                EntityType: "Process",
                EntityKey: processName,
                Category: ReflexionCategory.Absence,
                Details: ["Present in HLM only"],
                HlmMatches: hlmMatches,
                SmMatches: []
            );
        }

        // Divergence: SLM process not found in HLM
        foreach (
            var processName in smProcessNames
                .Where(p => string.IsNullOrWhiteSpace(p) is false)
                .Where(p => !hlmProcessNames.Contains(p) && !hlmProcessTargetNames.Contains(p))
        )
        {
            var smMatches = sm.Where(s =>
                    s.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)
                )
                .ToImmutableArray();

            yield return new ReflexionEntry(
                EntityType: "Process",
                EntityKey: processName,
                Category: ReflexionCategory.Divergence,
                Details: ["Present in SLM only"],
                HlmMatches: [],
                SmMatches: smMatches
            );
        }

        // Divergence: SLM process without mapping
        foreach (
            var smGroup in sm.Where(s => string.IsNullOrWhiteSpace(s.ProcessName))
                .GroupBy(s => s.EntryPoint.Trim())
        )
        {
            var smMatches = smGroup.ToImmutableArray();
            var entryPoint = smGroup.Key;

            yield return new ReflexionEntry(
                EntityType: "Process",
                EntityKey: entryPoint,
                Category: ReflexionCategory.Divergence,
                Details: ["Present in SLM only"],
                HlmMatches: [],
                SmMatches: smMatches
            );
        }
    }
}
