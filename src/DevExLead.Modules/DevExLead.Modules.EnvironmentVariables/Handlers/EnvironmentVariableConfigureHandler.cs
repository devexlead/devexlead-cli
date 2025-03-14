using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableConfigureHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var variables = UserStorageManager.GetUserStorage().EnvironmentVariables;

            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Key, VariableHelper.ReplacePlaceholders(variable.Value), EnvironmentVariableTarget.Machine);
                AnsiConsole.MarkupLine($"[green]{variable.Key} Environment variable configured on machine.[/]");
            }
        }
    }
}
