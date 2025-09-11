using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.EnvironmentVariables.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableListHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
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
