using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Model.Enums;
using DevExLead.Modules.Git.Helpers;
using Spectre.Console;
using static DevExLead.Core.Helpers.TerminalHelper;

namespace DevExLead.Modules.Git.Handlers
{
    internal class GitProfileHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var gitUsername = AnsiConsole.Ask<string>("Git Username: ");
            var gitEmail = AnsiConsole.Ask<string>("Git Email Address: ");

            //Global Profile
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git config --global user.name {gitUsername}");
            TerminalHelper.Run(PromptModeEnum.Powershell, "git config --global user.name");
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git config --global user.email {gitEmail}");
            TerminalHelper.Run(PromptModeEnum.Powershell, "git config --global user.email");

            //Set up Local Profile for all repositories
            var repositories = GitHelper.GetRepositories();
            foreach (var repository in repositories)
            {
                GitHelper.ConfigureProfile(repository, gitUsername, gitEmail);
            }
        }
    }
}
