using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableUpdateHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var key = ParameterHelper.ReadStringParameter(options, "key");
            var value = ParameterHelper.ReadStringParameter(options, "value");


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
