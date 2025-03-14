using DevEx.Core;
using DevEx.Core.Storage;
using Spectre.Console;

namespace DevEx.Modules.Command.Handlers
{
    internal class CommandListHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            var commands = userStorage.Commands;

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Body");
            table.AddColumn("Path");

            foreach (var command in commands.OrderBy(c => c.Name))
            {
                var name = command.Name ?? string.Empty;
                var body = command.Body ?? string.Empty;
                var path = command.Path ?? string.Empty;

                table.AddRow(name, body, path);
            }

            AnsiConsole.Write(table);
        }
    }
}
