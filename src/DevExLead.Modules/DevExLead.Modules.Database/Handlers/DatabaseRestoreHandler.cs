﻿using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.Database.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.Database.Handlers
{
    public class DatabaseRestoreHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var containerName = UserStorageManager.GetDecryptedValue("SqlContainerName");
            var backupLocation = UserStorageManager.GetDecryptedValue("SqlBackupLocation");
            var backupFilename = new FileInfo(backupLocation);
            var connectionString = UserStorageManager.GetDecryptedValue("SqlConnectionStringTemplate")
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
