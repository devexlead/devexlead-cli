using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevEx.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableDeleteHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var key = options["key"];

            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("--key is required for modify.");
                return;
            }

            EnvironmentVariableHelper.Delete(key);
            AnsiConsole.MarkupLine($"[green]Environment variable deleted.[/]");
        }
    }
}
