using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Command.Handlers
{
    internal class CommandUpdateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var name = options["name"];
            var path = options["path"];
            var body = options["body"];

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("--name --path --body are required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Commands.RemoveAll(c => c.Name == name);
            userStorage.Bookmarks.RemoveAll(b => b.Contains(name));

            var command = new Core.Storage.Model.Command()
            {
                Name = name,
                Path = path,
                Body = body
            };

            userStorage.Bookmarks.Add($"dxc command run --name \"{name}\"");
            userStorage.Commands.Add(command);

            UserStorageManager.SaveUserStorage(userStorage);

            Console.WriteLine($"Command has been updated.");
        }
    }
}
