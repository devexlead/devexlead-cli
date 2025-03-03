using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;

namespace DevEx.Modules.Configuration.Handlers.Vault
{
    public class WindowsVaultUpdateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var key = options["key"];
            var value = options["value"];

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("Both --key and --value are required for modify.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            value = EncryptionHelper.Encrypt(value);
            var entryExists = userStorage.Vault.Any(v => v.Key == key);

            if (entryExists)
            {
                userStorage.Vault[key] = value;
            }
            else
            {
                userStorage.Vault.Add(key, value);
            }

            UserStorageManager.SaveUserStorage(userStorage);
            Console.WriteLine($"Vault Entry Saved: Key={key}, Value={value}");
        }
    }
}
