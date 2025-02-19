using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Application.Handlers
{
    internal class ApplicationCreateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var name = options["name"];
            var path = options["path"];
            var command = options["command"];

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(command))
            {
                Console.WriteLine("--name --path --command are required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();
            var application = new Core.Storage.Model.Application()
            {
                Name = name,
                Path = path,
                RunCommand = command
            };

            userStorage.Applications.Add(application);

            UserStorageManager.SaveUserStorage(userStorage);

            Console.WriteLine($"Application has been created.");
        }
    }
}
