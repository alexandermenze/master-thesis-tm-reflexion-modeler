using System.Collections.Immutable;
using TMReflexionModeler.ReflexionModel.Core.Models;

namespace TMReflexionModeler.ReflexionModel.Csv;

public interface ICsvReaderService
{
    ImmutableArray<HlmEntity> LoadHlm(string path);

    ImmutableArray<SmEntity> LoadSm(string path);

    void WriteReflexion(string path, ImmutableArray<ReflexionEntry> entries);
}
