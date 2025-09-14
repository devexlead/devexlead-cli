using DevExLead.Cli;
using DevExLead.Core.Helpers;
using DevExLead.Core.Model.Command;
using DevExLead.Core.Storage;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

//--config parameter
if (OpenConfiguration())
{
    return;
}

// Read and parse the JSON file
string jsonFilePath = $"{AppContext.BaseDirectory}\\Commands.json"; // Ensure this file is placed in the project directory
string jsonContent = File.ReadAllText(jsonFilePath);
var dxcCommands = JsonSerializer.Deserialize<DxcCommands>(jsonContent);

var rootCommand = new RootCommand();
CommandsHelper.LoadCommands(rootCommand, dxcCommands);
CommandsHelper.UpdateCommandsBookmark(dxcCommands);

UserStorageManager.Initialize();

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseSuggestDirective();

var parser = builder.Build();

await parser.InvokeAsync(args);




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