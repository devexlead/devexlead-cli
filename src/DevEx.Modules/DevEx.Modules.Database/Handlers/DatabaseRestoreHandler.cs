using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using DevEx.Modules.Database.Helpers;

namespace DevEx.Modules.Database.Handlers
{
    public class DatabaseRestoreHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var containerName = UserStorageManager.GetDecryptedValue("SqlContainerName");
            var backupLocation = UserStorageManager.GetDecryptedValue("SqlBackupLocation");
            var backupFilename = new FileInfo(backupLocation);
            var masterConnectionString = UserStorageManager.GetDecryptedValue("SqlMasterConnectionString");
            var databaseName = UserStorageManager.GetDecryptedValue("SqlDatabaseName");

            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"docker cp {backupLocation} {containerName}:/var/opt/mssql/backup/");

            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"docker restart {containerName}");

            // Wait for 30 seconds
            Thread.Sleep(30000);

            var command = @$"RESTORE DATABASE {databaseName} 
                             FROM DISK = N'/var/opt/mssql/backup/{backupFilename.Name}' 
                             WITH MOVE 'DevExLead' TO '/var/opt/mssql/data/{databaseName}.mdf', 
                                  MOVE 'DevExLead_log' TO '/var/opt/mssql/data/{databaseName}.ldf', 
                             NOUNLOAD, 
                             REPLACE, 
                             RECOVERY;
                            ";
            
            DatabaseHelper.RunSqlCommand(masterConnectionString, command);
        }
    }
}
