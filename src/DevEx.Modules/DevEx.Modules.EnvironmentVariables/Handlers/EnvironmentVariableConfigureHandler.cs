using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using DevEx.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevEx.Modules.EnvironmentVariables.Handlers
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
