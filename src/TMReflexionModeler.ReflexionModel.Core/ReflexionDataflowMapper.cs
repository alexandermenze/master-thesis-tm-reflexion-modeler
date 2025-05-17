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

            // Search all SM entities with the same DataflowName and ProcessName as Source / Target
            // depending on the direction
            var matches = sm.Where(s =>
                    s.DataflowName.Equals(name, StringComparison.OrdinalIgnoreCase)
                    && (
                        (
                            s.Direction == DataflowDirection.Push
                            && s.ProcessName.Equals(
                                h.Flow.Source.Name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        || (
                            s.Direction == DataflowDirection.Pull
                            && s.ProcessName.Equals(
                                h.Flow.Target.Name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                )
                .ToImmutableArray();

            if (matches.Any())
                convergedSm.UnionWith(matches);

            var category = matches.Any()
                ? ReflexionCategory.Convergence
                : ReflexionCategory.Absence;

            var details = matches.Any() ? ImmutableArray<string>.Empty : ["Present in HLM only"];

            var key = $"{h.Flow.Source.Name}->{h.Flow.Name}->{h.Flow.Target.Name}";

            yield return new ReflexionEntry(
                EntityType: "Dataflow",
                EntityKey: key,
                Category: category,
                Details: details,
                HlmMatches: [h],
                SmMatches: matches
            );
        }

        // Divergence: All SM entities that were not converged
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
