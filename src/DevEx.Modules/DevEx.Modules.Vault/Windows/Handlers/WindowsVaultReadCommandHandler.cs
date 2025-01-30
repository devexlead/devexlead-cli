using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Vault.Windows.Handlers
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
            value = WindowsVaultHelper.Decrypt(value);
            Console.WriteLine($"Fetched item with Key={options["key"]} and Value={value}");
        }
    }
}
