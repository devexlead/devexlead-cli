using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;

namespace DevEx.Modules.Command.Handlers
{
    internal class CommandUpdateCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var name = options["name"];
            var body = options["body"];
            options.TryGetValue("path", out var path);

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
                Body = body
            };

            userStorage.Commands.Add(command);
            UserStorageManager.SaveUserStorage(userStorage);
        }
    }
}
