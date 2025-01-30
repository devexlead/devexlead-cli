using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Vault.Windows.Handlers
{

    public class WindowsVaultDeleteCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
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
