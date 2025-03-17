using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Model.Enums;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Command.Handlers
{
    public class CommandRunHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            options.TryGetValue("single", out var commandName);
            options.TryGetValue("multiple", out var commandGroup);

            if (string.IsNullOrWhiteSpace(commandName) && string.IsNullOrWhiteSpace(commandGroup))
            {
                Console.WriteLine("Specify either --single or --multiple option.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            if (!string.IsNullOrWhiteSpace(commandName))
            {
                RunSingle(commandName, userStorage);
            }

            if (!string.IsNullOrWhiteSpace(commandGroup))
            {
                await RunMultiple(commandGroup, userStorage);
            }   
        }

        private static void RunSingle(string commandName, UserStorage userStorage)
        {
            var command = userStorage.Commands.FirstOrDefault(a => a.Name == commandName);

            if (command == null)
            {
                Console.WriteLine("Command not found.");
                return;
            }

            command.Body = VariableHelper.ReplacePlaceholders(command.Body);
            TerminalHelper.Run(PromptModeEnum.Powershell, command.Body, command.Path);
        }

        private static async Task RunMultiple(string groupName, UserStorage userStorage)
        {
            var commands = userStorage.Commands.Where(a => a.Group == groupName).ToList();

            if (commands == null)
            {
                Console.WriteLine("Group not found.");
                return;
            }

            List<Task> tasks = new List<Task>();

            foreach (var command in commands)
            {
                command.Body = VariableHelper.ReplacePlaceholders(command.Body);
                var task = Task.Run(() => TerminalHelper.Run(PromptModeEnum.Powershell, command.Body, command.Path, isMultipleExecution: true));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
    }
}
