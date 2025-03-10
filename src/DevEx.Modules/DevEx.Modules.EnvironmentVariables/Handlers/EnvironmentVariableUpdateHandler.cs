using DevEx.Core;
using DevEx.Core.Helpers;
using Spectre.Console;

namespace DevEx.Modules.EnvironmentVariables.Handlers
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

            EnvironmentVariableHelper.AddEnvironmentVariable(key, value);
            AnsiConsole.MarkupLine($"[green]Environment variable updated.[/]");
        }
    }
}
