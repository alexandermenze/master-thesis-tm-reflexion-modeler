using System.Collections.Immutable;
using TMReflexionModeler.ReflexionModel.Core.Models;

namespace TMReflexionModeler.ReflexionModel.Core;

public static class ReflexionMapper
{
    public static ImmutableArray<ReflexionEntry> Map(
        ImmutableArray<HlmEntity> hlm,
        ImmutableArray<SmEntity> sm
    ) =>
        [
            .. ReflexionProcessMapper.MapProcesses(hlm, sm),
            .. ReflexionDataflowMapper.MapDataflows(hlm, sm),
        ];
}
