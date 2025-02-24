using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using Spectre.Console;

namespace DevEx.Modules.Database.Handlers
{
    public class DatabaseUpgradeHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var databaseName = UserStorageManager.GetDecryptedValue("SqlDatabaseName");
            var connectionString = UserStorageManager.GetDecryptedValue("SqlConnectionString")
                                                     .Replace("{{InitialCatalog}}", databaseName);
            var dacpacLocation = UserStorageManager.GetDecryptedValue("SqlDacpacLocation");

            var command = @$"SqlPackage /Action:Publish /SourceFile:'{dacpacLocation}' /TargetConnectionString:'{connectionString}'";
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, command);

            AnsiConsole.MarkupLine($"[green]Database Schema has been updated.[/]");
        }
    }
}
