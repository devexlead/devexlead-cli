using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Modules.Database.Handlers
{
    public class DatabaseUpgradeHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var databaseName = UserStorageManager.GetDecryptedValue("SqlDatabaseName");
            var connectionString = UserStorageManager.GetDecryptedValue("SqlConnectionStringTemplate")
                                                     .Replace("{{InitialCatalog}}", databaseName);
            var dacpacLocation = UserStorageManager.GetDecryptedValue("SqlDacpacLocation");
            var sqlDeploymentReportLocation = UserStorageManager.GetDecryptedValue("SqlDeploymentReportLocation");

            var reportCommand = @$"SqlPackage /Action:DeployReport /SourceFile:'{dacpacLocation}' /TargetConnectionString:'{connectionString}' /OutputPath:'{sqlDeploymentReportLocation}'";
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, reportCommand);

            var publishCommand = @$"SqlPackage /Action:Publish /SourceFile:'{dacpacLocation}' /TargetConnectionString:'{connectionString}' /p:DropObjectsNotInSource=True /p:BlockOnPossibleDataLoss=True";
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, publishCommand);

            AnsiConsole.MarkupLine($"[green]Database Schema has been updated.[/]");
        }
    }
}
