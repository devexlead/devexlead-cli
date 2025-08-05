using System.Text.Json;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Integrations.JIRA.Model;
using DevExLead.Modules.Jira.Helpers;
using DevExLead.Modules.Jira.Model;
using Spectre.Console;

namespace DevExLead.Modules.Jira.Handlers
{
    public class JiraWatchHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            try
            {
                bool isVerbose = ParameterHelper.ReadBoolParameter(options, "isVerbose");

                var jiraConnector = JiraHelper.GetJiraConnector(isVerbose, out string atlassianBaseUrl);

                var selectedSprint = JiraHelper.SelectSprint(jiraConnector);

                var jiraWatchJql = $"sprint = {selectedSprint?.Id}";
                AnsiConsole.MarkupLine($"[blue]Query: {jiraWatchJql} [/]");
                var jiraIssues = jiraConnector.RunJqlAsync(jiraWatchJql).Result;

                AnsiConsole.MarkupLine($"[blue]Watching Issues...[/]");
                await WatchIssues(atlassianBaseUrl, jiraConnector, jiraIssues);

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }



        private static async Task WatchIssues(string atlassianBaseUrl, JiraConnector jiraConnector, List<JiraIssue> jiraIssues)
        {
            var jiraWatchUserEmail = UserStorageManager.GetDecryptedValue("Jira:WatchUserEmail");

            foreach (var jiraIssue in jiraIssues)
            {
                AnsiConsole.MarkupLine($"[grey]~ {atlassianBaseUrl}/browse/{jiraIssue.Key} | {jiraIssue.Fields.IssueType.Name} | {jiraIssue.Fields.Summary}[/]");
                await jiraConnector.WatchIssueAsync(jiraIssue.Key, jiraWatchUserEmail);
            }
        }

        private static List<JiraIssue> FindDeletedIssues(List<JiraIssue> jiraIssues, List<JiraIssue>? plannedIssues)
        {
            var deletedIssues = plannedIssues == null
                                    ? new List<JiraIssue>()
                                    : plannedIssues.Where(p => !jiraIssues.Any(j => j.Key == p.Key)).ToList();

            return deletedIssues;
        }

        private static List<JiraIssue> FindAddedIssues(List<JiraIssue> jiraIssues, List<JiraIssue>? plannedIssues)
        {
            var addedIssues = jiraIssues
                                    .Where(j => plannedIssues == null || !plannedIssues.Any(p => p.Key == j.Key))
                                    .ToList();

           return addedIssues;
        }



        private static void SaveJsonFile(List<JiraIssue> jiraIssues, string filePath)
        {
            var json = JsonSerializer.Serialize(jiraIssues, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
            AnsiConsole.MarkupLine($"[yellow]Saved {jiraIssues.Count} issues to {filePath}[/]");
        }
    }
}
