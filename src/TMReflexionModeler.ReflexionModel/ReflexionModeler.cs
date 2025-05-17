using TMReflexionModeler.ReflexionModel.Core;
using TMReflexionModeler.ReflexionModel.Csv;

namespace TMReflexionModeler.ReflexionModel;

public class ReflexionModeler
{
    public static string Execute(string workDir, string hlmPath, string smPath)
    {
        Console.WriteLine("Starting Reflexion modeling...");
        Console.WriteLine($"Loading HLM from: {hlmPath}");
        Console.WriteLine($"Loading SLM from: {smPath}");

        var outPath = Path.Combine(workDir, "reflexion-model.csv");

        var csvService = new CsvReaderService();

        var hlmEntities = csvService.LoadHlm(hlmPath);
        var smEntities = csvService.LoadSm(smPath);

        Console.WriteLine(
            $"Loaded {hlmEntities.Length} HLM entities and {smEntities.Length} SLM entities."
        );

        var reflexionEntries = ReflexionMapper.Map(hlmEntities, smEntities);

        Console.WriteLine($"Generated {reflexionEntries.Length} reflexion entries.");
        Console.WriteLine($"Writing output to: {outPath}");

        csvService.WriteReflexion(outPath, reflexionEntries);

        Console.WriteLine("Reflexion modeling completed successfully.");

        return outPath;
    }
}
