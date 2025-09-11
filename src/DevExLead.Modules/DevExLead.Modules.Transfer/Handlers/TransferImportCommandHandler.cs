using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.Transfer.Helpers;
using Spectre.Console;
using System.Text.Json;

namespace DevExLead.Modules.Transfer.Handlers
{
    public class TransferImportCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            TransferHelper.SelectPath(out string folderPath);

            //Import Configuration
            var configurationPath = Path.Combine(folderPath, "configuration.json");
            var configuration = FileHelper.ReadFile(configurationPath);
            var userStorage = JsonSerializer.Deserialize<UserStorage>(configuration);

            //Import Keys
            var keyPath = Path.Combine(folderPath, "dxc.key");
            var encryptionKey = FileHelper.ReadFile(keyPath);
            userStorage.EncryptionKeys = SecurityHelper.EncryptKey(encryptionKey);

            UserStorageManager.SaveUserStorage(userStorage);

            //Delete files after importing
            File.Delete(configurationPath); 
            File.Delete(keyPath);

            AnsiConsole.MarkupLine($"[green]Configuration and encryption key imported from {folderPath}[/]");
        }
    }
}
