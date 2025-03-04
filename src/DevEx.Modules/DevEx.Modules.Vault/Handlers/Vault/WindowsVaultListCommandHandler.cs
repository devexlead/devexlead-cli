using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using Spectre.Console;

namespace DevEx.Modules.Configuration.Handlers.Vault
{
    public class WindowsVaultListCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            var vaultItems = userStorage.Vault;

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Decrypted Value");
            table.AddColumn("Encrypted Value");

            foreach (var item in vaultItems.OrderBy(c => c.Key))
            {
                table.AddRow(item.Key, EncryptionHelper.Decrypt(item.Value), item.Value);
            }

            AnsiConsole.Write(table);
        }
    }
}
