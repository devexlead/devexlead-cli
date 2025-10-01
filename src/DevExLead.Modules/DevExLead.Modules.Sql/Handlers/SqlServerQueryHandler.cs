using System.Data;
using System.Text.Json;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace DevExLead.Modules.Sql.Handlers
{
    public class SqlServerQueryHandler: ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var queryPath = UserStorageManager.GetDecryptedValue("SqlServer:QueryPath");
            var connectionString = UserStorageManager.GetDecryptedValue("SqlServer:ConnectionString");

            var scriptLocation = await SelectSqlScriptWithAutocomplete(queryPath);
            if (string.IsNullOrEmpty(scriptLocation))
            {
                AnsiConsole.MarkupLine("[yellow]No script selected. Operation cancelled.[/]");
                return;
            }

            // Display selected script info
            AnsiConsole.MarkupLine($"[green]Executing SQL script:[/] [cyan]{Path.GetFileName(scriptLocation)}[/]");
            AnsiConsole.MarkupLine($"[gray]Location: {scriptLocation}[/]");

            if (!File.Exists(scriptLocation))
            {
                AnsiConsole.MarkupLine($"[red]SQL script file not found: {scriptLocation}[/]");
                return;
            }

            //replace parameters
            var sql = File.ReadAllText(scriptLocation);

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                var dataTable = new DataTable();
                dataTable.Load(reader);

                // Build Spectre.Console table dynamically
                var table = new Table().Border(TableBorder.Rounded).Expand();

                // Add columns
                foreach (DataColumn column in dataTable.Columns)
                {
                    table.AddColumn(column.ColumnName);
                }

                // Add rows
                foreach (DataRow row in dataTable.Rows)
                {
                    var cells = new string[dataTable.Columns.Count];
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        cells[i] = row[i]?.ToString() ?? "";
                    }
                    table.AddRow(cells);
                }

                // Display result count
                AnsiConsole.MarkupLine($"\n[green]Query executed successfully. {dataTable.Rows.Count} row(s) returned.[/]");

                // Render
                AnsiConsole.Write(table);
            }
            catch (SqlException ex)
            {
                AnsiConsole.MarkupLine($"[red]SQL Error: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
        }

        private async Task<string> SelectSqlScriptWithAutocomplete(string queryPath)
        {
            var sqlFiles = Directory.GetFiles(queryPath, "*.sql", SearchOption.AllDirectories);
            var choices = sqlFiles.ToList();
            choices.Add("[red]Cancel[/]");

            var selectedChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select a SQL script to execute:[/]")
                    .PageSize(15)
                    .MoreChoicesText("[grey](Move up and down to reveal more scripts)[/]")
                    .AddChoices(choices)
                    .UseConverter(choice => 
                    {
                        if (choice == "[red]Cancel[/]") return "Cancel";
                        return choice;
                    })
            );

            if (selectedChoice == "[red]Cancel[/]")
            {
                return string.Empty;
            }

            return selectedChoice ?? string.Empty;
        }

        private string BrowseForSqlFile()
        {
            try
            {
                AnsiConsole.MarkupLine("[yellow]Please enter the path to your SQL file:[/]");

                var filePath = AnsiConsole.Ask<string>("File path:");
                
                if (string.IsNullOrEmpty(filePath))
                {
                    return string.Empty;
                }

                // Expand relative paths and environment variables
                filePath = Environment.ExpandEnvironmentVariables(filePath);
                
                if (!Path.IsPathRooted(filePath))
                {
                    filePath = Path.GetFullPath(filePath);
                }

                if (!File.Exists(filePath))
                {
                    AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
                    return string.Empty;
                }

                if (!filePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                {
                    var proceed = AnsiConsole.Confirm($"[yellow]The selected file does not have a .sql extension. Continue anyway?[/]");
                    if (!proceed)
                    {
                        return string.Empty;
                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error browsing for file: {ex.Message}[/]");
                return string.Empty;
            }
        }
    }
}
