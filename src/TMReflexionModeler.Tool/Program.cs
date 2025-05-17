using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TMReflexionModeler.Tool;

var tdmOption = new Option<string>(
    aliases: ["--tdmf", "--threat-dragon-model-file"],
    description: "Path to the Threat Dragon model JSON file"
)
{
    IsRequired = true,
};

var tddOption = new Option<string?>(
    aliases: ["--tdd", "--threat-dragon-diagram"],
    description: "Name of the diagram within the model file"
);

var sourceDirOption = new Option<string>(
    aliases: ["--source-dir"],
    description: "Path to the system's source code directory"
)
{
    IsRequired = true,
};

var excludeDirsOption = new Option<string?>(
    aliases: ["--exclude-dirs"],
    description: "Semicolon-separated list of folder paths from which all .csproj files should be excluded"
);

var slnOption = new Option<string>(
    aliases: ["--sln", "--solution-file"],
    description: "Path to the solution (.sln) file"
)
{
    IsRequired = true,
};

var excludedExternalCalls = new Option<string?>(
    aliases: ["--exclude-calls-file"],
    description: "Path to file containing new-line separated external calls to exclude from the the source model"
);

var rootCommand = new RootCommand
{
    tdmOption,
    tddOption,
    sourceDirOption,
    excludeDirsOption,
    slnOption,
    excludedExternalCalls,
};

rootCommand.Description =
    "TMReflexionModeler: Generate a reflexion model from a Threat Dragon model and a C# solution using CodeQL";

rootCommand.Handler = CommandHandler.Create<string, string?, string, string?, string, string?>(
    (tdmFile, tdd, sourceDir, excludeDirs, slnFile, excludedExternalCallsFile) =>
    {
        Console.WriteLine($"Threat Dragon Model File: {tdmFile}");

        if (string.IsNullOrEmpty(tdd) is false)
            Console.WriteLine($"Diagram Name: {tdd}");

        Console.WriteLine($"Solution File: {slnFile}");

        if (string.IsNullOrEmpty(excludeDirs) is false)
            Console.WriteLine($"Excluded Directories: {excludeDirs}");

        Console.WriteLine($"Source Code Directory: {sourceDir}");

        if (string.IsNullOrWhiteSpace(excludedExternalCallsFile) is false)
            Console.WriteLine($"Source Code Directory: {sourceDir}");

        return new ReflexionModelOrchestrator().Run(
            tdmFile,
            tdd,
            sourceDir,
            excludeDirs,
            slnFile,
            excludedExternalCallsFile
        );
    }
);

return await rootCommand.InvokeAsync(args);
