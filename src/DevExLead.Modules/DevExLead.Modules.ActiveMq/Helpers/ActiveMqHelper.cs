using DevExLead.Core.Model.Enums;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Modules.ActiveMq.Helpers
{
    public static class ActiveMqHelper
    {
        public static EnvironmentEnum SelectEnvironment()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<EnvironmentEnum>()
                    .Title("[green]Select ActiveMQ environment:[/]")
                    .AddChoices(EnvironmentEnum.Development, EnvironmentEnum.Stage, EnvironmentEnum.Production)
                    .UseConverter(env => env.ToString())
            );
        }

        public static (string? brokerUri, string? username, string? password) GetConnectionDetails(EnvironmentEnum environment)
        {
            var environmentPrefix = environment.ToString();
            var brokerUri = UserStorageManager.GetDecryptedValue($"ActiveMq:{environmentPrefix}:ConnectionString");
            var username = UserStorageManager.GetDecryptedValue($"ActiveMq:{environmentPrefix}:Username");
            var password = UserStorageManager.GetDecryptedValue($"ActiveMq:{environmentPrefix}:Password");

            return (brokerUri, username, password);
        }
    }
}