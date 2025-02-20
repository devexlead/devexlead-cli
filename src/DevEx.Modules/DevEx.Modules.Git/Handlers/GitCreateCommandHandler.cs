using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Git.Handlers
{
    internal class GitCreateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var name = options["name"];
            var remoteLocation = options["remoteLocation"];
            var workingFolder = options["workingFolder"];
            var defaultBranch = options["defaultBranch"];

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(remoteLocation) || string.IsNullOrWhiteSpace(workingFolder) ||
                string.IsNullOrWhiteSpace(defaultBranch))
            {
                Console.WriteLine("--name --remoteLocation --workingFolder --defaultBranch are required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Repositories.RemoveAll(a => a.Name == name);

            var repository = new Core.Storage.Model.Repository()
            {
                Name = name,
                RemoteLocation = remoteLocation,
                WorkingFolder = workingFolder,
                DefaultBranch = defaultBranch
            };

            userStorage.Repositories.Add(repository);

            UserStorageManager.SaveUserStorage(userStorage);

            Console.WriteLine($"Repository has been created.");
        }
    }
}
