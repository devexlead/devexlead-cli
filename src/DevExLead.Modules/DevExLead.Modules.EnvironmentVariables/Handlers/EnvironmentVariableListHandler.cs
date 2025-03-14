using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using DevEx.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevEx.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableListHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var environmentVariables = EnvironmentVariableHelper.List();

            var table = new Table();
            table.AddColumn("Key");
            table.AddColumn("Value");

            foreach (var item in environmentVariables.OrderBy(c => c.Key))
            {
                table.AddRow(item.Key, item.Value);
            }

            AnsiConsole.Write(table);
        }
    }
}
