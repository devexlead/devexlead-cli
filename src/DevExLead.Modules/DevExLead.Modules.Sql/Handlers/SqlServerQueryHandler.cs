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
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var sqlScriptName = ParameterHelper.ReadStringParameter(options, "key");
            var sqlParams = ParameterHelper.ReadStringParameter(options, "parameters");

            var connectionString = UserStorageManager.GetDecryptedValue("SqlServer:ConnectionString");
            var scriptLocation = UserStorageManager.GetUserStorage().Applications.SqlServer.Queries.FirstOrDefault(q => q.Key.Equals(sqlScriptName))?.Location;


            //replace parameters
            var sql = File.ReadAllText(scriptLocation);
            sqlParams = sqlParams.Replace("'", "\"");
            Dictionary<string, string>? queryParameters = JsonSerializer.Deserialize<Dictionary<string, string>>(sqlParams);

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

            // Render
            AnsiConsole.Write(table);
        }
    }
}
