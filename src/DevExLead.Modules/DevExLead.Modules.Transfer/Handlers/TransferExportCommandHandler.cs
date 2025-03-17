using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.Transfer.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.Transfer.Handlers
{
    public class TransferExportCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            string folderPath;
            var keys = SecurityHelper.DecryptKey(UserStorageManager.GetUserStorage().EncryptionKeys);
            TransferHelper.SelectPath(out folderPath);
            var filePath = Path.Combine(folderPath, "dxc.key");
            FileHelper.SaveFile(filePath, keys);
            AnsiConsole.MarkupLine($"[green]Keys exported to {filePath}[/]");
        }
    }
}
