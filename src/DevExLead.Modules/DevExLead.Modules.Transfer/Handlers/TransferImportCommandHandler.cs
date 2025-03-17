using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.Transfer.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.Transfer.Handlers
{
    public class TransferImportCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            string folderPath;
            TransferHelper.SelectPath(out folderPath);
            var filePath = Path.Combine(folderPath, "dxc.key");
            var encryptionKey = FileHelper.ReadFile(filePath);
            var userStorage = UserStorageManager.GetUserStorage();
            userStorage.EncryptionKeys = SecurityHelper.EncryptKey(encryptionKey);
            UserStorageManager.SaveUserStorage(userStorage);
            File.Delete(filePath); //Delete the key file after importing
            AnsiConsole.MarkupLine($"[green]Encryption Keys were imported[/]");
        }
    }
}
