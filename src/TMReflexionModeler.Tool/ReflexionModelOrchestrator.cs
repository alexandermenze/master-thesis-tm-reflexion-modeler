using TMReflexionModeler.CodeQLSourceExtract;
using TMReflexionModeler.SolutionManipulation;
using TMReflexionModeler.ThreatDragonExtract;

namespace TMReflexionModeler.Tool;

public class ReflexionModelOrchestrator
{
    public async Task<int> Run(
        string threatDragonModelFile,
        string? threatDragonDiagramName,
        string sourceDir,
        string? excludeDirs,
        string solutionFile,
        string? excludedExternalCallsFile
    )
    {
        var workDir = Directory.CreateDirectory("tm-rm-work");

        var hlmPath = await ExtractThreatModel(
            workDir.FullName,
            threatDragonModelFile,
            threatDragonDiagramName
        );

        if (excludeDirs is not null)
            await SolutionManipulator.RemoveProjects(solutionFile, excludeDirs);

        var smPath = await CodeQLSourceExtractor.Extract(
            workDir.FullName,
            sourceDir,
            excludedExternalCallsFile
        );

        return 0;
    }

    private static async Task<string> ExtractThreatModel(
        string tmpDir,
        string threatDragonModelFile,
        string? threatDragonDiagramName
    )
    {
        var outputFilePath = Path.Combine(tmpDir, "td-out.csv");

        await ThreatDragonFormatConverter.Convert(
            threatDragonModelFile,
            outputFilePath,
            threatDragonDiagramName
        );

        return outputFilePath;
    }
}
