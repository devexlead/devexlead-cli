using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DevExLead.Cli;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

//--config parameter
if (OpenConfiguration())
{
    return;
}

var rootCommand = new RootCommand();

Helper.LoadCommands(rootCommand);

UserStorageManager.Initialize();

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseSuggestDirective();

var parser = builder.Build();

await parser.InvokeAsync(args);

//Refresh Intellisense Commands
IntelliSenseHelper.ResetPsReadLineFile();



return;

static bool OpenConfiguration()
{
    string[] commandLineArgs = Environment.GetCommandLineArgs();

    if (commandLineArgs.Contains("--config"))
    {
        string userSecretsId = "6b2e2731-a735-438e-bf6f-e749e0ebcd02";
        string secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            userSecretsId,
            "secrets.json"
        );

        if (File.Exists(secretsPath))
        {
            // Launch VS Code with the secrets.json file
            Process.Start(new ProcessStartInfo
            {
                FileName = "code",
                Arguments = $"\"{secretsPath}\"",
                UseShellExecute = true
            });
        }
        else
        {
            Console.WriteLine($"secrets.json not found at: {secretsPath}");
        }

        return true;
    }

    return false;
}