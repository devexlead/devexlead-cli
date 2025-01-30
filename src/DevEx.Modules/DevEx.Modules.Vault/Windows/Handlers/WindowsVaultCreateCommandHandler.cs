using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Vault.Windows.Handlers
{
    public class WindowsVaultCreateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var key = options["key"];
            var value = options["value"];

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("Both --key and --value are required for create.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();
            value = WindowsVaultHelper.Encrypt(value);
            userStorage.Vault.Add(key, value);
            UserStorageManager.SaveUserStorage(userStorage);

            Console.WriteLine($"Created item: Key={key}, New Value={value}");
        }

    }
}
