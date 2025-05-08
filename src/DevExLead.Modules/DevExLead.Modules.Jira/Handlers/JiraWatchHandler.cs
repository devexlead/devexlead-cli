using DevExLead.Core;
using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using Spectre.Console;

namespace DevExLead.Modules.Jira.Handlers
{
    public class JiraWatchHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            try
            {
                bool isVerbose = false;
                if (options.TryGetValue("isVerbose", out var isVerboseString))
                {
                    if (!bool.TryParse(isVerboseString, out isVerbose))
                    {
                        throw new ArgumentException("Invalid value for isVerbose. It must be a boolean.");
                    }
                }

                var atlassianBaseUrl = UserStorageManager.GetDecryptedValue("Atlassian:BaseUrl");
                if (atlassianBaseUrl == null) return;

                var atlassianUser = UserStorageManager.GetDecryptedValue("Atlassian:User");
                if (atlassianUser == null) return;

                var atlassianKey = UserStorageManager.GetDecryptedValue("Atlassian:Key");
                if (atlassianKey == null) return;

                var jiraConnector = new JiraConnector(atlassianBaseUrl, atlassianUser, atlassianKey, isVerbose);

                var jiraWatchJql = UserStorageManager.GetDecryptedValue("Jira:WatchJql");
                var jiraIssues = jiraConnector.RunJqlAsync(jiraWatchJql).Result;
                AnsiConsole.MarkupLine($"[green]Query: {jiraWatchJql} [/]");

                var jiraWatchUserEmail = UserStorageManager.GetDecryptedValue("Jira:WatchUserEmail");

                foreach (var jiraIssue in jiraIssues)
                {
                    AnsiConsole.MarkupLine($"[green]Watching: {atlassianBaseUrl}/browse/{jiraIssue.Key} [/]");
                    await jiraConnector.WatchIssueAsync(jiraIssue.Key, jiraWatchUserEmail);
                }

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }

    }
}
