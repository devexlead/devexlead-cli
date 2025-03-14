using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Modules.Database.Handlers
{
    public class DatabaseSeedHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var databaseName = UserStorageManager.GetDecryptedValue("SqlDatabaseName");
            var connectionString = UserStorageManager.GetDecryptedValue("SqlConnectionStringTemplate")
                                                     .Replace("{{InitialCatalog}}", databaseName);
            var sqlDatabaseSeedCommand = UserStorageManager.GetDecryptedValue("SqlDatabaseSeedCommand");
            var sqlDatabaseSeedPath = UserStorageManager.GetDecryptedValue("SqlDatabaseSeedPath");
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"{sqlDatabaseSeedCommand} '{connectionString}'", sqlDatabaseSeedPath);
            AnsiConsole.MarkupLine($"[green]{databaseName} has been seeded.[/]");
        }
    }
}
