using System.Collections.Immutable;
using TMReflexionModeler.ReflexionModel.Core.Models;

namespace TMReflexionModeler.ReflexionModel.Core;

public class ReflexionDataflowMapper
{
    public static IEnumerable<ReflexionEntry> MapDataflows(
        ImmutableArray<HlmEntity> hlm,
        ImmutableArray<SmEntity> sm
    )
    {
        var convergedSm = new HashSet<SmEntity>();

        foreach (var h in hlm)
        {
            var name = h.Flow.Name.Trim();

            var pullMatch = sm.Where(s => s.Direction is DataflowDirection.Pull)
                .Where(s => s.DataflowName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.ProcessName == h.Flow.Target.Name)
                .ToImmutableArray();

            var pushMatch = sm.Where(s => s.Direction is DataflowDirection.Push)
                .Where(s => s.DataflowName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.ProcessName == h.Flow.Source.Name)
                .ToImmutableArray();

            ImmutableArray<SmEntity> smMatches = [];

            if (h.Flow.Source.Type is "Process" && h.Flow.Target.Type is "Process")
            {
                if (pullMatch.IsEmpty is false && pushMatch.IsEmpty is false)
                    // Convergence
                    smMatches = [.. pushMatch, .. pullMatch];
            }
            else if (h.Flow.Source.Type is "Process")
            {
                if (pushMatch.IsEmpty is false)
                    // Convergence
                    smMatches = [.. pushMatch];
            }
            else if (h.Flow.Target.Type is "Process")
            {
                if (pullMatch.IsEmpty is false)
                    // Convergence
                    smMatches = [.. pullMatch];
            }

            convergedSm.UnionWith(smMatches);

            var details = smMatches.IsEmpty
                ? ["Present in HLM only"]
                : ImmutableArray<string>.Empty;

            var key = $"{h.Flow.Source.Name}->{h.Flow.Name}->{h.Flow.Target.Name}";
            
            var category = smMatches.IsEmpty
                ? ReflexionCategory.Absence
                : ReflexionCategory.Convergence;

            yield return new ReflexionEntry(
                EntityType: "Dataflow",
                EntityKey: key,
                Category: category,
                Details: details,
                HlmMatches: [h],
                SmMatches: smMatches
            );
        }

        // Divergence: All Source Model entities that did not converge
        foreach (var s in sm.Except(convergedSm))
        {
            var processName = string.IsNullOrWhiteSpace(s.ProcessName)
                ? s.EntryPoint
                : s.ProcessName;

            var key = s.Direction switch
            {
                DataflowDirection.Push => $"{processName}->{s.DataflowName}->...",
                DataflowDirection.Pull => $"...->{s.DataflowName}->{processName}",
                DataflowDirection.Unknown => $"{processName}:{s.InternalCall}:{s.ExternalCall}",
                _ => throw new InvalidOperationException("Unhandled dataflow direction!"),
            };

            yield return new ReflexionEntry(
                EntityType: "Dataflow",
                EntityKey: key,
                Category: ReflexionCategory.Divergence,
                Details: ["Present in SLM only"],
                HlmMatches: [],
                SmMatches: [s]
            );
        }
    }
}
