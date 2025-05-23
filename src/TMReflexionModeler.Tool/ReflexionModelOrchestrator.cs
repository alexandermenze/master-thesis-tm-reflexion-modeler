using System.Diagnostics;
using TMReflexionModeler.CodeQLSourceExtract;
using TMReflexionModeler.ReflexionModel;
using TMReflexionModeler.SarifFormatter;
using TMReflexionModeler.SolutionManipulation;
using TMReflexionModeler.ThreatDragonExtract;

namespace TMReflexionModeler.Tool;

public static class ReflexionModelOrchestrator
{
    public static async Task<int> Run(
        string threatDragonModelFile,
        string? threatDragonDiagramName,
        string sourceDir,
        string? excludeDirs,
        string solutionFile,
        string? excludedExternalCallsFile
    )
    {
        try
        {
            Console.WriteLine("Starting ReflexionModelOrchestrator...");

            var pwd = Directory.GetCurrentDirectory();

            var workDir = Directory.CreateDirectory("tm-rm-work");
            Console.WriteLine($"Work directory created: {workDir}");

            // Stage 1: Convert High-Level Model Format
            var hlmPath = await RunStageAsync(
                "Convert High Level Model Format",
                () =>
                    ExtractThreatModel(
                        workDir.FullName,
                        threatDragonModelFile,
                        threatDragonDiagramName
                    )
            );

            // Stage 2: Exclude Projects from Solution
            if (!string.IsNullOrWhiteSpace(excludeDirs))
            {
                await RunStageAsync(
                    "Exclude Projects from Solution",
                    () => SolutionManipulator.RemoveProjects(solutionFile, excludeDirs)
                );
            }

            // Stage 3: Extract Source Model and Mappings
            var smPath = await RunStageAsync(
                "Extract Source Model and Mappings",
                () =>
                    CodeQLSourceExtractor.Extract(
                        workDir.FullName,
                        sourceDir,
                        excludedExternalCallsFile
                    )
            );

            // Stage 4: Execute Reflexion Modeling
            var rmPath = await RunStageAsync(
                "Execute Reflexion Modeling",
                () => Task.Run(() => ReflexionModeler.Execute(workDir.FullName, hlmPath, smPath))
            );

            // Stage 5: Convert to SARIF
            var rmSarifOutcome = await RunStageAsync(
                "Convert to SARIF",
                () => SarifOutputFormatter.ConvertToSarif(workDir.FullName, rmPath, pwd, hlmPath)
            );

            Console.WriteLine($"Reflexion model csv generated at: {rmPath}");
            Console.WriteLine(
                $"Reflexion model sarif generated at: {rmSarifOutcome.SarifFilePath}"
            );
            return rmSarifOutcome.ReportCount is 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"An error occurred during orchestration: {ex}");
            return -1;
        }
    }

    private static async Task<T> RunStageAsync<T>(string stageName, Func<Task<T>> action)
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine();
        Console.WriteLine($" === Stage '{stageName}' started ===");
        Console.WriteLine();
        try
        {
            var result = await action();
            sw.Stop();
            Console.WriteLine(
                $" === Stage '{stageName}' completed in {sw.ElapsedMilliseconds}ms ==="
            );
            Console.WriteLine();
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            await Console.Error.WriteLineAsync(
                $" === Stage '{stageName}' failed after {sw.ElapsedMilliseconds}ms: {ex} ==="
            );
            throw;
        }
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
