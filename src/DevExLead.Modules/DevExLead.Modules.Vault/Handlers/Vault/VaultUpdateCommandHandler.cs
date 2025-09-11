using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Configuration.Handlers.Vault
{
    public class VaultUpdateCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var key = ParameterHelper.ReadStringParameter(options, "key");
            var value = ParameterHelper.ReadStringParameter(options, "value");

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("Both --key and --value are required for modify.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            value = SecurityHelper.EncryptVaultEntry(value);
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
