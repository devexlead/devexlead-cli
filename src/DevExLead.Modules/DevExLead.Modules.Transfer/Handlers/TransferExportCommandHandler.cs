using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.Transfer.Helpers;
using Spectre.Console;
using System.Text.Json;

namespace DevExLead.Modules.Transfer.Handlers
{
    public class TransferExportCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            TransferHelper.SelectPath(out string folderPath);

            //Export Configuration
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var configuration = JsonSerializer.Serialize(userStorage, jsonOptions);
            var configurationPath = Path.Combine(folderPath, "configuration.json");
            FileHelper.SaveFile(configurationPath, configuration);

            //Export Keys
            var keys = SecurityHelper.DecryptKey(userStorage.EncryptionKeys);
            var keyPath = Path.Combine(folderPath, "dxc.key");
            FileHelper.SaveFile(keyPath, keys);

            AnsiConsole.MarkupLine($"[green]Configuration and encryption key exported to {folderPath}[/]");
        }
    }
}
