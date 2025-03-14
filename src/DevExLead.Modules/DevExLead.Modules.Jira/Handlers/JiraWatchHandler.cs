using DevExLead.Core;
using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Modules.Jira.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.Jira.Handlers
{
    public class JiraWatchHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            try
            {
                //bool isVerbose = false;
                //if (options.TryGetValue("isVerbose", out var isVerboseString))
                //{
                //    if (!bool.TryParse(isVerboseString, out isVerbose))
                //    {
                //        throw new ArgumentException("Invalid value for isVerbose. It must be a boolean.");
                //    }
                //}

                //var atlassianBaseUrl = UserStorageManager.GetDecryptedValue("AtlassianBaseUrl");
                //if (atlassianBaseUrl == null) return;

                //var atlassianUser = UserStorageManager.GetDecryptedValue("AtlassianUser");
                //if (atlassianUser == null) return;

                //var atlassianKey = UserStorageManager.GetDecryptedValue("AtlassianKey");
                //if (atlassianKey == null) return;

                //var jiraConnector = new JiraConnector(atlassianBaseUrl, atlassianUser, atlassianKey, isVerbose);

                //var jiraIssue = JiraHelper.SelectParent(jiraConnector);

                //await jiraConnector.WatchIssueAsync(jiraIssue.Key, "attlasianUserId");

                //AnsiConsole.MarkupLine($"[green]{atlassianBaseUrl}/browse/{result.Key}[/]");

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }

    }
}
