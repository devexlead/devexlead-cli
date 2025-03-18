using DevExLead.Core;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Command.Handlers
{
    internal class CommandDeleteCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var name = options["key"];

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("--name is required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Commands.RemoveAll(c => c.Key == name);
            UserStorageManager.SaveUserStorage(userStorage);
        }
    }
}
