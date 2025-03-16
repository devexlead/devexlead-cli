using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Command.Handlers
{
    internal class CommandUpdateCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var name = options["name"];
            var body = options["body"];

            options.TryGetValue("path", out var path);
            options.TryGetValue("group", out var group);

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("--name --body are required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Commands.RemoveAll(c => c.Name == name);

            var command = new Core.Storage.Model.Command()
            {
                Name = name,
                Path = path,
                Body = body,
                Group = group
            };

            userStorage.Commands.Add(command);
            UserStorageManager.SaveUserStorage(userStorage);
        }
    }
}
