using System.Text.Json;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Integrations.JIRA.Model;
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
                bool isSnapshot = ParameterHelper.ReadBoolParameter(options, "isSnapshot");

                var atlassianBaseUrl = UserStorageManager.GetDecryptedValue("Atlassian:BaseUrl");
                if (atlassianBaseUrl == null) return;

                var atlassianUser = UserStorageManager.GetDecryptedValue("Atlassian:User");
                if (atlassianUser == null) return;

                var atlassianKey = UserStorageManager.GetDecryptedValue("Atlassian:Key");
                if (atlassianKey == null) return;

                var jiraConnector = new JiraConnector(atlassianBaseUrl, atlassianUser, atlassianKey, isVerbose);

                var jiraBoardId = int.Parse(UserStorageManager.GetDecryptedValue("Atlassian:BoardId"));
                var jiraSprints = jiraConnector.FetchSprints(jiraBoardId).Result;

                var currentSprint = jiraSprints.FirstOrDefault(s => s.State == "active");
                var jiraWatchJql = $"sprint = {currentSprint?.Id}";
                AnsiConsole.MarkupLine($"[green]Query: {jiraWatchJql} [/]");
                var jiraIssues = jiraConnector.RunJqlAsync(jiraWatchJql).Result;

                var filePath = Path.Combine(Environment.CurrentDirectory, "jira-issues.json");
                if (isSnapshot)
                {
                    SaveJsonFile(jiraIssues, filePath);
                }
                else
                {
                    var json = File.ReadAllText(filePath);
                    var plannedIssues = JsonSerializer.Deserialize<List<JiraIssue>>(json);

                    FindAddedIssues(jiraIssues, plannedIssues);
                    FindDeletedIssues(jiraIssues, plannedIssues);
                    FindReestimatedIssues(jiraIssues, plannedIssues);
                }

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
                AnsiConsole.MarkupLine($"[green]Watching: {atlassianBaseUrl}/browse/{jiraIssue.Key} ({jiraIssue.Fields.Summary}) [/]");
                await jiraConnector.WatchIssueAsync(jiraIssue.Key, jiraWatchUserEmail);
            }
        }

        private static void FindDeletedIssues(List<JiraIssue> jiraIssues, List<JiraIssue>? plannedIssues)
        {
            var deletedIssues = plannedIssues == null
                                    ? new List<JiraIssue>()
                                    : plannedIssues.Where(p => !jiraIssues.Any(j => j.Key == p.Key)).ToList();

            AnsiConsole.MarkupLine($"[yellow]Deleted Issues: {deletedIssues.Count}[/]");
            foreach (var issue in deletedIssues)
            {
                AnsiConsole.MarkupLine($"[red]- {issue.Key}: {issue.Fields.Summary}[/]");
            }
        }

        private static void FindAddedIssues(List<JiraIssue> jiraIssues, List<JiraIssue>? plannedIssues)
        {
            var addedIssues = jiraIssues
                                    .Where(j => plannedIssues == null || !plannedIssues.Any(p => p.Key == j.Key))
                                    .ToList();

            // Output results
            AnsiConsole.MarkupLine($"[yellow]Added Issues: {addedIssues.Count}[/]");
            foreach (var issue in addedIssues)
            {
                AnsiConsole.MarkupLine($"[green]+ {issue.Key}: {issue.Fields.Summary}[/]");
            }
        }

        private static void FindReestimatedIssues(List<JiraIssue> jiraIssues, List<JiraIssue>? plannedIssues)
        {
            if (plannedIssues == null) return;

            var reestimatedIssues = jiraIssues
                .Join(plannedIssues,
                      current => current.Key,
                      planned => planned.Key,
                      (current, planned) => new
                      {
                          Key = current.Key,
                          Summary = current.Fields.Summary,
                          OldEstimate = planned.Fields.Points,
                          NewEstimate = current.Fields.Points
                      })
                .Where(x => x.OldEstimate != x.NewEstimate)
                .ToList();

            AnsiConsole.MarkupLine($"[yellow]Re-estimated Issues: {reestimatedIssues.Count}[/]");
            foreach (var issue in reestimatedIssues)
            {
                AnsiConsole.MarkupLine(
                    $"[blue]* {issue.Key}: {issue.Summary}[/] [grey](Estimate changed from {issue.OldEstimate ?? 0} to {issue.NewEstimate ?? 0})[/]");
            }
        }

        private static void SaveJsonFile(List<JiraIssue> jiraIssues, string filePath)
        {
            var json = JsonSerializer.Serialize(jiraIssues, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
            AnsiConsole.MarkupLine($"[yellow]Saved {jiraIssues.Count} issues to {filePath}[/]");
        }
    }
}
