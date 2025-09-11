using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableDeleteHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var key = ParameterHelper.ReadStringParameter(options, "key");

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
