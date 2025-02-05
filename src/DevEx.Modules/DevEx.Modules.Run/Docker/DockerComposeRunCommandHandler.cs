using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using Spectre.Console;

namespace DevEx.Modules.Run.Docker
{
    public class DockerComposeRunCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            var dockerComposePath = string.Empty;

            try
            {
                dockerComposePath = userStorage.Vault["DockerComposePath"];
                dockerComposePath = EncryptionHelper.Decrypt(dockerComposePath);
            }
            catch (Exception)
            {
                AnsiConsole.WriteLine("DockerComposePath Not Found in Vault");
                return;
            }

            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, "docker-compose down", dockerComposePath);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, "docker-compose up -d", dockerComposePath);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, "docker-compose logs nginx", dockerComposePath);
        }
    }
}
