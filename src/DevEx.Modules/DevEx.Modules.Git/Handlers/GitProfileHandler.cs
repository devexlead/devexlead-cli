using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Modules.Git.Helpers;
using Spectre.Console;
using static DevEx.Core.Helpers.TerminalHelper;

namespace DevEx.Modules.Git.Handlers
{
    internal class GitProfileHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var gitUsername = AnsiConsole.Ask<string>("Git Username: ");
            var gitEmail = AnsiConsole.Ask<string>("Git Email Address: ");

            //Global Profile
            TerminalHelper.Run(ConsoleMode.Powershell, $"git config --global user.name {gitUsername}");
            TerminalHelper.Run(ConsoleMode.Powershell, "git config --global user.name");
            TerminalHelper.Run(ConsoleMode.Powershell, $"git config --global user.email {gitEmail}");
            TerminalHelper.Run(ConsoleMode.Powershell, "git config --global user.email");

            //Set up Local Profile for all repositories
            var repositories = GitHelper.GetRepositories();
            foreach (var repository in repositories)
            {
                GitHelper.ConfigureProfile(repository, gitUsername, gitEmail);
            }
        }
    }
}
