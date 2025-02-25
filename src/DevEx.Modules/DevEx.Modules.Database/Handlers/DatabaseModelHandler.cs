using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using Spectre.Console;

namespace DevEx.Modules.Database.Handlers
{
    public class DatabaseModelHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var databaseName = UserStorageManager.GetDecryptedValue("SqlDatabaseName");
            var connectionString = UserStorageManager.GetDecryptedValue("SqlConnectionString")
                                                     .Replace("{{InitialCatalog}}", databaseName);
            var sqlEntityFrameworkProjectPath = UserStorageManager.GetDecryptedValue("SqlEntityFrameworkProjectPath");
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"dotnet ef dbcontext scaffold '{connectionString}' -f --project '{sqlEntityFrameworkProjectPath}'");
            AnsiConsole.MarkupLine($"[green]{databaseName} Entity Framework Model has been updated.[/]");
        }
    }
}
