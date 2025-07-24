using System.Text.Json;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Integrations.JIRA.Model;
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

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Added to Sprint...[/]");
                    var addedIssues = FindAddedIssues(jiraIssues, plannedIssues);
                    addedIssues.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[green]+ {atlassianBaseUrl}/browse/{jiraIssue.Key} |  {jiraIssue.Fields.IssueType.Name}  |  {jiraIssue.Fields.Summary}[/]"));

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Removed from Sprint...[/]");
                    var deletedIssues = FindDeletedIssues(jiraIssues, plannedIssues);
                    deletedIssues.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[red]- {atlassianBaseUrl}/browse/{jiraIssue.Key} | {jiraIssue.Fields.IssueType.Name} | {jiraIssue.Fields.Summary}[/]"));

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Re-Estimated during Sprint...[/]");
                    var reestimatedIssues = FindReestimatedIssues(jiraIssues, plannedIssues);
                    reestimatedIssues.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[yellow]* {atlassianBaseUrl}/browse/{jiraIssue.Key}  | {jiraIssue.IssueType.Name} | {jiraIssue.Summary}[/] [grey](Estimate changed from {jiraIssue.OldEstimate ?? 0} to {jiraIssue.NewEstimate ?? 0})[/]"));
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[blue]Issues with no estimation...[/]");
                var nonEstimatedTickets = jiraIssues.Where(i => !i.Fields.IssueType.Name.Equals("Sub-task"))
                                                    .Where(i => i.Fields.Points == null || i.Fields.Points == 0)
                                                    .ToList();
                nonEstimatedTickets.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[grey]~ {atlassianBaseUrl}/browse/{jiraIssue.Key} | {jiraIssue.Fields.IssueType.Name} | {jiraIssue.Fields.Summary}[/]"));

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[blue]Watching issues in the current sprint...[/]");
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

        private static List<JiraIssueReestimate> FindReestimatedIssues(List<JiraIssue> jiraIssues, List<JiraIssue>? plannedIssues)
        {
            var reestimatedIssues = jiraIssues
                .Join(plannedIssues,
                      current => current.Key,
                      planned => planned.Key,
                      (current, planned) => new JiraIssueReestimate()
                      {
                          Key = current.Key,
                          Summary = current.Fields.Summary,
                          IssueType = current.Fields.IssueType,
                          OldEstimate = planned.Fields.Points,
                          NewEstimate = current.Fields.Points
                      })
                .Where(x => x.OldEstimate != x.NewEstimate)
                .ToList();

            return reestimatedIssues;
        }

        private static void SaveJsonFile(List<JiraIssue> jiraIssues, string filePath)
        {
            var json = JsonSerializer.Serialize(jiraIssues, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
            AnsiConsole.MarkupLine($"[yellow]Saved {jiraIssues.Count} issues to {filePath}[/]");
        }
    }
}
