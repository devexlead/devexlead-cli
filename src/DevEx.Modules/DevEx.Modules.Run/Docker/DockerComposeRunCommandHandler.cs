using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;

namespace DevEx.Modules.Run.Docker
{
    public class DockerComposeRunCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var dockerComposePath = UserStorageManager.GetDecryptedValue("DockerComposePath");
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, "docker-compose down", dockerComposePath);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, "docker-compose up -d", dockerComposePath);
        }
    }
}
