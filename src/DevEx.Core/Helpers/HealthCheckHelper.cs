using Spectre.Console;
using System.Net;
using System.Net.NetworkInformation;

namespace DevEx.Core.Helpers
{
    public class HealthCheckHelper
    {
        public static void CheckEnvironmentVariables()
        {
            Console.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Checking Environment Variables...[/]");

            var requiredVariables = EnvironmentVariableHelper.GetEnvironmentVariables();

            foreach (var variable in requiredVariables)
            {
                var storedVariableValue = Environment.GetEnvironmentVariable(variable.Key);
                if (!string.IsNullOrEmpty(storedVariableValue) &&
                    storedVariableValue.Equals(variable.Value))
                {
                    AnsiConsole.MarkupLine($"[green]{variable.Key}: OK[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{variable.Key}: Failed. Value should be {variable.Value}[/]");
                }
            }
        }

        //private static void CheckRedisHealth()
        //{
        //    Console.WriteLine();
        //    AnsiConsole.MarkupLine($"[yellow]Checking Redis...[/]");
        //    try
        //    {
        //        var redisPort = ContainerHelper.GetContainersAsync().Result.FirstOrDefault(c => c.Name.Equals("Redis")).PortBindings[0].Host;
        //        var redisConnectionString = $"127.0.0.1:{redisPort}";
        //        var redisConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        //        var redisDatabase = redisConnectionMultiplexer.GetDatabase();
        //        redisDatabase.Ping();
        //        AnsiConsole.MarkupLine($"[green]Redis: OK[/]");
        //    }
        //    catch (Exception ex)
        //    {
        //        AnsiConsole.MarkupLine($"[red]Redis: {ex.Message}[/]");
        //    }
        //}

        public static void CheckEndpointHealth(string name, string endpoint, HttpMethod httpMethod)
        {
            Console.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Checking {name}...[/]");
            try
            {
                var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
                var response = new HttpResponseMessage();
                switch (httpMethod.Method)
                {
                    case "GET":
                        response = httpClient.GetAsync(endpoint).Result;
                        break;

                    case "POST":
                        response = httpClient.PostAsync(endpoint, null).Result;
                        break;
                }

                if (response.StatusCode.Equals(HttpStatusCode.OK) ||
                    response.StatusCode.Equals(HttpStatusCode.Unauthorized))
                {
                    AnsiConsole.MarkupLine($"[green]{name}: OK[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{name}: HTTP {response.StatusCode}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{name}: {ex.Message}[/]");
            }
        }

        public static void PingServer(string name, string endpoint)
        {
            var pinger = new Ping();
            PingReply reply = pinger.Send(endpoint);
            var pingable = reply.Status == IPStatus.Success;
            if (pingable)
            {
                AnsiConsole.MarkupLine($"[green]{name}: OK[/]");

            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{name}: failed.[/]");
            }
        }
        //public static void CheckSqlServerHealth()
        //{
        //    Console.WriteLine();
        //    AnsiConsole.MarkupLine($"[yellow]Checking SQL Server...[/]");
        //    try
        //    {
        //        var sqlConnection = new SqlConnection(connectionString);
        //        sqlConnection.Open();
        //        sqlConnection.Close();
        //        AnsiConsole.MarkupLine($"[green]SQL Server: OK[/]");
        //    }
        //    catch (Exception ex)
        //    {
        //        AnsiConsole.MarkupLine($"[red]SQL Server: {ex.Message}[/]");
        //    }
        //}


    }
}
