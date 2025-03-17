using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Command.Handlers
{
    internal class CommandUpdateCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var key = options["key"];
            var body = options["body"];

            options.TryGetValue("path", out var path);
            options.TryGetValue("group", out var group);
            options.TryGetValue("process", out var process);

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("--name --body are required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Commands.RemoveAll(c => c.Key == key);

            var command = new Core.Storage.Model.Command()
            {
                Key = key,
                Path = path,
                Body = body,
                Group = group,
                Process = process
            };

            userStorage.Commands.Add(command);
            UserStorageManager.SaveUserStorage(userStorage);
        }
    }
}
