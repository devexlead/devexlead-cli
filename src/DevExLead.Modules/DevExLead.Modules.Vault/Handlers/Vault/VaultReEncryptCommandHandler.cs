using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Modules.Configuration.Handlers.Vault
{
    public class VaultReEncryptCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var userStorage = UserStorageManager.GetUserStorage();

            //decrypt vault entries
            var decryptedItems = new Dictionary<string, string>();
            foreach (var item in userStorage.Vault)
            {
                var decryptedValue = UserStorageManager.GetDecryptedValue(item.Key);
                decryptedItems.Add(item.Key, decryptedValue);
            }

            //Generate new encryption key
            var reEncryptionKey = SecurityHelper.GenerateRSAKeys();

            //re-encrypt vault entries
            foreach (var item in decryptedItems)
            {
                userStorage.Vault[item.Key] = SecurityHelper.EncryptVaultEntry(item.Value, reEncryptionKey);
            }

            userStorage.EncryptionKeys = reEncryptionKey;

            UserStorageManager.SaveUserStorage(userStorage);

            AnsiConsole.MarkupLine("[green]Vault entries re-encrypted.[/]");
        }

       
    }
}
