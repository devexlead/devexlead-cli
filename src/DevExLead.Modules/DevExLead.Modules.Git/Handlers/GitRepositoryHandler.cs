using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Git.Handlers
{
    internal class GitRepositoryHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var key = ParameterHelper.ReadStringParameter(options, "key");
            var remoteLocation = ParameterHelper.ReadStringParameter(options, "remoteLocation");
            var workingFolder = ParameterHelper.ReadStringParameter(options, "workingFolder");
            var defaultBranch = ParameterHelper.ReadStringParameter(options, "defaultBranch");

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(remoteLocation) || string.IsNullOrWhiteSpace(workingFolder) ||
                string.IsNullOrWhiteSpace(defaultBranch))
            {
                Console.WriteLine("--key --remoteLocation --workingFolder --defaultBranch are required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Repositories.RemoveAll(a => a.Key == key);

            var repository = new Core.Storage.Model.Repository()
            {
                Key = key,
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
