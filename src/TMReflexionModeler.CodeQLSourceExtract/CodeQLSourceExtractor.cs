using System.Diagnostics;

namespace TMReflexionModeler.CodeQLSourceExtract;

public static class CodeQLSourceExtractor
{
    public static async Task<string> Extract(
        string workDir,
        string sourceDir,
        string? excludedExternalCallsFile
    )
    {
        await RunProcess("codeql", "--version");

        var exeDir = AppContext.BaseDirectory;
        var queriesDir = Path.Combine(exeDir, "codeql-query-sources");

        var queryFilePath = Path.Combine(queriesDir, "identify-call-paths-with-tags.ql");

        if (!File.Exists(queryFilePath))
            throw new FileNotFoundException($"CodeQL query file not found: '{queryFilePath}'");

        var methodFilterFile = string.IsNullOrWhiteSpace(excludedExternalCallsFile)
            ? Path.Combine(queriesDir, "empty.txt")
            : excludedExternalCallsFile;

        if (File.Exists(methodFilterFile) is false)
            throw new FileNotFoundException($"Method filter file not found: '{methodFilterFile}'");

        var dbPath = Path.Combine(workDir, "codeql-db");
        var bqrsPath = Path.Combine(workDir, "codeql-out.bqrs");
        var csvOutputPath = Path.Combine(workDir, "codeql-out.csv");

        await RunProcess("codeql", $"database create -l csharp -s \"{sourceDir}\" -- \"{dbPath}\"");

        var queryArgs =
            $"query run --database \"{dbPath}\" --output \"{bqrsPath}\" "
            + $"--external methodFilter=\"{methodFilterFile}\" -- \"{queryFilePath}\"";

        await RunProcess("codeql", queryArgs);

        await RunProcess(
            "codeql",
            $"bqrs decode -r \"#select\" -o \"{csvOutputPath}\" --format=csv -- \"{bqrsPath}\""
        );

        return csvOutputPath;
    }

    private static async Task RunProcess(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using var proc = new Process();
        proc.StartInfo = psi;

        try
        {
            proc.Start();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new InvalidOperationException(
                $"The executable '{fileName}' was not found on your PATH. "
                    + "Please install CodeQL and ensure it’s available in your PATH."
            );
        }

        proc.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
                Console.WriteLine(e.Data);
        };
        proc.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
                Console.Error.WriteLine(e.Data);
        };

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Command `{fileName} {arguments}` exited with code {proc.ExitCode}."
            );
        }
    }
}
