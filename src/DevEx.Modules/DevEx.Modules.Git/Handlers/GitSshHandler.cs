using DevEx.Core;
using DevEx.Modules.Git.Helpers;
using Spectre.Console;

namespace DevEx.Modules.Git.Handlers
{
    public class GitSshHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var gitEmail = AnsiConsole.Ask<string>("Git Email Address: ");
            GitHelper.ConfigureSSH(gitEmail);
        }
    }
}
