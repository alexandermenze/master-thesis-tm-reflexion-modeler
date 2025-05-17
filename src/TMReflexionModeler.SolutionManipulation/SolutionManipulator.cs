using System.Diagnostics;

namespace TMReflexionModeler.SolutionManipulation;

public static class SolutionManipulator
{
    public static async Task RemoveProjects(string solutionFilePath, string excludeDirs)
    {
        var dirs = excludeDirs.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var dir in dirs)
        {
            Console.WriteLine($"→ Removing all .csproj in: {dir}");

            if (Directory.Exists(dir) is false)
            {
                await Console.Error.WriteLineAsync($"Error: dir '{dir}' does not exist.");
                continue;
            }

            var foundAny = false;

            foreach (
                var projFile in Directory.EnumerateFiles(
                    dir,
                    "*.csproj",
                    SearchOption.AllDirectories
                )
            )
            {
                foundAny = true;
                Console.WriteLine($"   • {projFile}");

                var arguments = $"sln \"{solutionFilePath}\" remove \"{projFile}\"";

                var psi = new ProcessStartInfo("dotnet", arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var proc = Process.Start(psi);

                if (proc is null)
                    throw new InvalidOperationException("Failed to start 'dotnet' process.");

                await proc.WaitForExitAsync();

                if (proc.ExitCode is 0)
                    continue;

                var err = (await proc.StandardError.ReadToEndAsync()).Trim();

                await Console.Error.WriteLineAsync(
                    $"Error: Failed to remove project '{projFile}' from solution.{Environment.NewLine}{err}"
                );
            }

            if (!foundAny)
            {
                await Console.Error.WriteLineAsync(
                    $"Warning: No .csproj files found in dir '{dir}'."
                );
            }

            Console.WriteLine();
        }
    }
}
