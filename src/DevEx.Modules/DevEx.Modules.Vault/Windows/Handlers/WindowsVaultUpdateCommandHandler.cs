using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Vault.Windows.Handlers
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
            userStorage.Vault[key] = WindowsVaultHelper.Encrypt(value);
            UserStorageManager.SaveUserStorage(userStorage);

            Console.WriteLine($"Modified item: Key={key}, New Value={value}");
        }
    }
}
