using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Modules.Configuration.Handlers.Vault
{
    public class VaultTransferCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            string folderPath;
            options.TryGetValue("export", out var export);
            options.TryGetValue("import", out var encryptionKeyPath);

            if (!options.ContainsKey("export") && !options.ContainsKey("import"))
            {
                Console.WriteLine("Either --export or --import is required.");
                return;
            }

            if (options.ContainsKey("export"))
            {
                var keys = SecurityHelper.DecryptKey(UserStorageManager.GetUserStorage().EncryptionKeys);
                SelectPath(out folderPath);
                var filePath = Path.Combine(folderPath, "dxc.key");
                FileHelper.SaveFile(filePath, keys);
                AnsiConsole.MarkupLine($"[green]Keys exported to {filePath}[/]");
            }

            if (options.ContainsKey("import"))
            {
                SelectPath(out folderPath);
                var filePath = Path.Combine(folderPath, "dxc.key");
                var encryptionKey = FileHelper.ReadFile(filePath);
                var userStorage = UserStorageManager.GetUserStorage();
                userStorage.EncryptionKeys = SecurityHelper.EncryptKey(encryptionKey);
                UserStorageManager.SaveUserStorage(userStorage);
                File.Delete(filePath); //Delete the key file after importing
                AnsiConsole.MarkupLine($"[green]Encryption Keys were imported[/]");
            }
        }

        private static void SelectPath(out string folderPath)
        {
            folderPath = AnsiConsole.Ask<string>("Please enter the folder location:");
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                AnsiConsole.MarkupLine("[red]Invalid folder location.[/]");
                return;
            }
        }
    }
}
