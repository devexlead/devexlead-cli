using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using DevEx.Modules.Database.Helpers;
using Spectre.Console;

namespace DevEx.Modules.Database.Handlers
{
    public class DatabaseRestoreHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var containerName = UserStorageManager.GetDecryptedValue("SqlContainerName");
            var backupLocation = UserStorageManager.GetDecryptedValue("SqlBackupLocation");
            var backupFilename = new FileInfo(backupLocation);
            var connectionString = UserStorageManager.GetDecryptedValue("SqlConnectionString")
                                                     .Replace("{{InitialCatalog}}", "master");
            var databaseName = UserStorageManager.GetDecryptedValue("SqlDatabaseName");

            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"docker cp {backupLocation} {containerName}:/var/opt/mssql/backup/");

            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"docker restart {containerName}");

            // Wait for 10 seconds
            Thread.Sleep(10000);

            var command = @$"RESTORE DATABASE {databaseName} 
                             FROM DISK = N'/var/opt/mssql/backup/{backupFilename.Name}' 
                             WITH MOVE 'DevExLead' TO '/var/opt/mssql/data/{databaseName}.mdf', 
                                  MOVE 'DevExLead_log' TO '/var/opt/mssql/data/{databaseName}.ldf', 
                             NOUNLOAD, 
                             REPLACE, 
                             RECOVERY;
                            ";
            
            DatabaseHelper.RunSqlCommand(connectionString, command);

            AnsiConsole.MarkupLine($"[green]{databaseName} has been restored.[/]");
        }
    }
}
