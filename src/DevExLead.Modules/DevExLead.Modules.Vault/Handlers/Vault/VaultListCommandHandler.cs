using DevExLead.Core;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Modules.Configuration.Handlers.Vault
{
    public class VaultListCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            var vaultItems = userStorage.Vault;

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Decrypted Value");
            table.AddColumn("Encrypted Value");

            foreach (var item in vaultItems.OrderBy(c => c.Key))
            {
                table.AddRow(item.Key, UserStorageManager.GetDecryptedValue(item.Key), item.Value);
            }

            AnsiConsole.Write(table);
        }
    }
}
