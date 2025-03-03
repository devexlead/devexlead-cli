using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using System.Text.RegularExpressions;

namespace DevEx.Modules.Command.Handlers
{
    public class CommandRunHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
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

            command.Body = ReplacePlaceholders(command.Body);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, command.Body, command.Path);
        }

        private string ReplacePlaceholders(string input)
        {
            return Regex.Replace(input, @"\{\{(\w+)\}\}", match =>
            {
                var key = match.Groups[1].Value;
                return UserStorageManager.GetDecryptedValue(key) ?? match.Value;
            });
        }
    }
}
