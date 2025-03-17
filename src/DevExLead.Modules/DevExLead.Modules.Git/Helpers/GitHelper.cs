using DevExLead.Core.Helpers;
using DevExLead.Core.Model.Enums;
using DevExLead.Core.Storage;
using DevExLead.Core.Storage.Model;
using HandlebarsDotNet;
using Spectre.Console;
using TextCopy;

namespace DevExLead.Modules.Git.Helpers
{
    public static class GitHelper
    {
        public static void ConfigureProfile(Repository repository, string gitUsername, string gitEmail)
        {
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git config --local user.name {gitUsername}", repository.WorkingFolder);
            TerminalHelper.Run(PromptModeEnum.Powershell, "git config --local user.name", repository.WorkingFolder);
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git config --local user.email {gitEmail}", repository.WorkingFolder);
            TerminalHelper.Run(PromptModeEnum.Powershell, "git config --local user.email", repository.WorkingFolder);
        }

        public static void Clone(Repository repository)
        {
            if (!Directory.Exists(repository.WorkingFolder))
            {
                Directory.CreateDirectory(repository.WorkingFolder);
            }
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git clone {repository.RemoteLocation} {repository.WorkingFolder}");
        }

        public static void CreateBranch(string path, string issueId)
        {
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git branch {issueId}", path);
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git checkout {issueId}", path);
            TerminalHelper.Run(PromptModeEnum.Powershell, $"git push --set-upstream origin {issueId}", path);
        }

        public static void GetLatest(Repository repository)
        {
            var askToProceed = AnsiConsole.Ask<string>($"All your local changes in the {repository.Name} will be stashed. Do you want to proceed? (y/n)");
            if (askToProceed.ToLower().Equals("y"))
            {
                TerminalHelper.Run(PromptModeEnum.Powershell, $"git stash", repository.WorkingFolder);
                TerminalHelper.Run(PromptModeEnum.Powershell, $"git reset --hard", repository.WorkingFolder);
                TerminalHelper.Run(PromptModeEnum.Powershell, $"git fetch", repository.WorkingFolder);
                TerminalHelper.Run(PromptModeEnum.Powershell, $"git checkout {repository.DefaultBranch}", repository.WorkingFolder);
                TerminalHelper.Run(PromptModeEnum.Powershell, $"git pull", repository.WorkingFolder);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Operation has been cancelled[/]");
            }
        }

        public static List<Repository> GetRepositories()
        {
            var userStorage = UserStorageManager.GetUserStorage();
            return userStorage.Repositories;
        }

        public static string GetSshPath()
        {
            string sslConfigPath = Path.Combine(AppContext.BaseDirectory, "SSH");
            return sslConfigPath;
        }

        public static void ConfigureSSH(string emailAddress)
        {
            var userProfileFolder = Environment.GetEnvironmentVariable("USERPROFILE");
            var keyName = "DevExLead";

           //Clean Existing SSH Key
            var sshKeyFile = $"{userProfileFolder}\\.ssh\\{keyName}";
            if (File.Exists(sshKeyFile))
            {
                File.Delete(sshKeyFile);
                File.Delete($"{sshKeyFile}.pub");
            }

            //Copy known_hosts File
            var knownHostsFile = $"{userProfileFolder}\\.ssh\\known_hosts";
            File.Copy($"{GitHelper.GetSshPath()}\\known_hosts", knownHostsFile, true);

            //Generate config file based on Template
            var sshConfigTemplateFile = $"{GitHelper.GetSshPath()}\\config.template";
            string templateContent = File.ReadAllText(sshConfigTemplateFile);
            var template = Handlebars.Compile(templateContent);
            var data = new { keyName = keyName };
            string result = template(data);
            File.WriteAllText($"{GitHelper.GetSshPath()}\\config", result);

            //Copy config File
            var sshConfigDestinationFile = $"{userProfileFolder}\\.ssh\\config";
            File.Copy($"{GitHelper.GetSshPath()}\\config", sshConfigDestinationFile, true);

            //Generate SSH Key
            TerminalHelper.Run(PromptModeEnum.Powershell, $"ssh-keygen -t ed25519 -C {emailAddress} -f \"{sshKeyFile}\"");

            //Restart SSH Agent
            TerminalHelper.Run(PromptModeEnum.Powershell, "Stop-Service ssh-agent");
            TerminalHelper.Run(PromptModeEnum.Powershell, "Set-Service -Name ssh-agent -StartupType Automatic");
            TerminalHelper.Run(PromptModeEnum.Powershell, "Start-Service ssh-agent");
            TerminalHelper.Run(PromptModeEnum.Powershell, "Get-Service -Name ssh-agent");
            
            string publicKey = File.ReadAllText($"{sshKeyFile}.pub");
            ClipboardService.SetText(publicKey);

            AnsiConsole.MarkupLine($"[green]The public key has been copied to the clipboard. Go to https://github.com/settings/ssh/new to add it to your GitHub account.[/]");

            //List all identities: ssh-add -L
            //Delete all identities: ssh-add -D
        }

        //internal static async Task RevertChanges(string revertTicketId, string branchName, string owner, string repositoryName)
        //{
        //    var repositories = GetRepositories();
        //    var repository = repositories.FirstOrDefault(r => r.Name.Equals(repositoryName));

        //    //Ensure there are no pending changes
        //    TerminalHelper.Run(ConsoleMode.Powershell, $"git reset --hard", repository.WorkingFolder);

        //    var gitHubConnector = new GitHubConnector();
        //    var commits = await gitHubConnector.GetCommits(owner, repository.Name, branchName);
        //    commits = commits.Where(c => c.commit.message.Contains(revertTicketId)).ToList();
        //    foreach (var commit in commits)
        //    {
        //        TerminalHelper.Run(ConsoleMode.Powershell, $"git revert {commit.sha}", repository.WorkingFolder);
        //    }

        //    GitHelper.SyncUpSharedService(repository.WorkingFolder);

        //    TerminalHelper.Run(ConsoleMode.Powershell, $"git push", repository.WorkingFolder);
        //}
    }
}