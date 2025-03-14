using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using System.Text.RegularExpressions;

namespace DevExLead.Modules.Command.Handlers
{
    public class CommandRunHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var commandName = options["name"];

            if (string.IsNullOrWhiteSpace(commandName))
            {
                Console.WriteLine("--name is required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();
            var command = userStorage.Commands.FirstOrDefault(a => a.Name == commandName);

            if (command == null)
            {
                Console.WriteLine("Command not found.");
                return;
            }

            command.Body = VariableHelper.ReplacePlaceholders(command.Body);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, command.Body, command.Path);
        }
    }
}
