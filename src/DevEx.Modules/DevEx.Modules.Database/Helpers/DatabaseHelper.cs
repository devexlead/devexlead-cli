using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace DevEx.Modules.Database.Helpers
{
    public class DatabaseHelper
    {
        public static void RunSqlCommand(string connectionString, string sql, List<SqlParameter> parameters = null)
        {
            AnsiConsole.MarkupLine($"[yellow]{sql}[/]");

            var sqlConnection = new SqlConnection(connectionString);

            using (sqlConnection)
            {
                sqlConnection.Open();

                var sqlCommand = new SqlCommand(sql, sqlConnection);
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());
                }
                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
            }
        }
    }
}

