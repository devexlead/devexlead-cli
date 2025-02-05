using DevEx.Core;
using DevEx.Modules.IntelliSense.Helpers;
using Spectre.Console;

namespace DevEx.Modules.IntelliSense
{
    public class CleanHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            //Clean PSReadLine File
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string psReadLineFile = Path.Combine(userFolder, "AppData\\Roaming\\Microsoft\\Windows\\PowerShell\\PSReadLine\\ConsoleHost_history.txt");
            File.WriteAllText(psReadLineFile, string.Empty);

            //Insert DevEx CLI Commands into PSReadLine File
            var commands = IntelliSenseHelper.GetCommandLinesFromFile($"{AppContext.BaseDirectory}\\Commands.json");
            File.AppendAllLines(psReadLineFile, commands);
            AnsiConsole.MarkupLine($"[Green]DevEx CLI IntelliSense is updated.[/]");
        }
    }
}
