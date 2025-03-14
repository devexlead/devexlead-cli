using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableUpdateHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var key = options["key"];
            var value = options["value"];

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("Both --key and --value are required for modify.");
                return;
            }

            EnvironmentVariableHelper.Add(key, value);
            AnsiConsole.MarkupLine($"[green]Environment variable updated.[/]");
        }
    }
}
