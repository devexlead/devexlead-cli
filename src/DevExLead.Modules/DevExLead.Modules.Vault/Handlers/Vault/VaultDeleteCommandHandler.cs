using DevExLead.Core;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Configuration.Handlers.Vault
{

    public class VaultDeleteCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var key = options["key"];

            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("--key is required for delete.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();
            userStorage.Vault.Remove(key);
            UserStorageManager.SaveUserStorage(userStorage);
            Console.WriteLine($"Deleted item with Key={options["key"]}");
        }


    }
}
