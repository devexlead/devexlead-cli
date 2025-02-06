using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;

namespace DevEx.Modules.Configuration.Handlers.Vault
{
    public class WindowsVaultReadCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var key = options["key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("--key is required for read.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();
            var value = userStorage.Vault.FirstOrDefault(v => v.Key.Equals(key)).Value;
            value = EncryptionHelper.Decrypt(value);
            Console.WriteLine($"Fetched item with Key={options["key"]} and Value={value}");
        }
    }
}
