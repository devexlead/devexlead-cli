using DevExLead.Core;
using DevExLead.Modules.Git.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.Git.Handlers
{
    public class GitSshHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var gitEmail = AnsiConsole.Ask<string>("Git Email Address: ");
            GitHelper.ConfigureSSH(gitEmail);
        }
    }
}
