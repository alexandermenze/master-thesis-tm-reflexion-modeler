using System.Collections.Immutable;

namespace TMReflexionModeler.ReflexionModel.Csv;

public static class MultiValueConverter
{
    public static ImmutableArray<string> SplitAndNormalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return
        [
            .. input
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLowerInvariant()),
        ];
    }
}
