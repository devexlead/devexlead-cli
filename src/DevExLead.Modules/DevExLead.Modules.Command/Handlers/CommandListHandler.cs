using DevExLead.Core;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Modules.Command.Handlers
{
    internal class CommandListHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            var commands = userStorage.Commands;

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Body");
            table.AddColumn("Path");
            table.AddColumn("Group");
            table.AddColumn("Process");

            foreach (var command in commands.OrderBy(c => c.Key))
            {
                var name = command.Key ?? string.Empty;
                var body = command.Body ?? string.Empty;
                var path = command.Path ?? string.Empty;
                var group = command.Group ?? string.Empty;
                var process = command.Process ?? string.Empty;

                table.AddRow(name, body, path, group, process);
            }

            AnsiConsole.Write(table);
        }
    }
}
