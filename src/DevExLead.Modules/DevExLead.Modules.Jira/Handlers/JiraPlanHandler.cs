using System.Text.Json;
using System.Text.RegularExpressions;
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
    public class JiraPlanHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            try
            {
                bool isVerbose = ParameterHelper.ReadBoolParameter(options, "isVerbose");
                bool isSnapshot = ParameterHelper.ReadBoolParameter(options, "isSnapshot");
                bool isChangeTracker = ParameterHelper.ReadBoolParameter(options, "isChangeTracker");
                bool isSprintPlanning = ParameterHelper.ReadBoolParameter(options, "isSprintPlanning");

                var jiraConnector = JiraHelper.GetJiraConnector(isVerbose, out string atlassianBaseUrl);

                var selectedSprint = JiraHelper.SelectSprint(jiraConnector);

                var jiraWatchJql = $"sprint = {selectedSprint?.Id}";
                AnsiConsole.MarkupLine($"[blue]Query: {jiraWatchJql} [/]");
                var jiraIssues = jiraConnector.RunJqlAsync(jiraWatchJql).Result;

                var appFolder = AppContext.BaseDirectory;
                var filePath = Path.Combine(appFolder, $"jira-issues-{selectedSprint?.Id}.json");

                if (isSnapshot)
                {
                    SaveJsonFile(jiraIssues, filePath);
                }
                else if (isChangeTracker)
                {
                    AnsiConsole.MarkupLine($"[yellow]Read {jiraIssues.Count} issues from {filePath}[/]");
                    var json = File.ReadAllText(filePath);
                    var plannedIssues = JsonSerializer.Deserialize<List<JiraIssue>>(json);

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Added to Sprint...[/]");
                    var addedIssues = FindAddedIssues(jiraIssues, plannedIssues);
                    addedIssues.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[green]+ {atlassianBaseUrl}/browse/{jiraIssue.Key} | {jiraIssue.Fields.Reporter.DisplayName} | {jiraIssue.Fields.Points ?? 0} | {jiraIssue.Fields.IssueType.Name}  | {jiraIssue.Fields.Summary} | {jiraIssue.Fields.Priority.Name}[/]"));

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Removed from Sprint...[/]");
                    var deletedIssues = FindDeletedIssues(jiraIssues, plannedIssues);
                    deletedIssues.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[red]- {atlassianBaseUrl}/browse/{jiraIssue.Key} | {jiraIssue.Fields.IssueType.Name} | {jiraIssue.Fields.Summary} | {jiraIssue.Fields.Priority.Name} | {jiraIssue.Fields.Points} | {jiraIssue.Fields.Assignee?.DisplayName}[/]"));

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Re-Estimated during Sprint...[/]");
                    var reestimatedIssues = FindReestimatedIssues(jiraIssues, plannedIssues);
                    reestimatedIssues.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[yellow]* {atlassianBaseUrl}/browse/{jiraIssue.Key}  | {jiraIssue.IssueType.Name} | {jiraIssue.Summary}[/] [grey](Estimate changed from {jiraIssue.OldEstimate ?? 0} to {jiraIssue.NewEstimate ?? 0}) | {jiraIssue.Assignee}[/]"));
                }
                else if (isSprintPlanning)
                {
                    var totalStoryPoints = jiraIssues.Where(i => i.Fields.Points != null)
                                                     .Sum(i => CalculateRemainingPoints(i.Fields.Labels, i.Fields.Points));

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Total Story Points: {totalStoryPoints}[/]");

                    var ticketWithRemainingPoints = jiraIssues.Where(i => HasRemainingPoints(i.Fields.Labels)).ToList();

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Issues with remaining points...[/]");

                    ticketWithRemainingPoints.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[grey]~ {atlassianBaseUrl}/browse/{jiraIssue.Key} | {jiraIssue.Fields.Reporter.DisplayName ?? "Unassigned"} | {jiraIssue.Fields.IssueType.Name} | {jiraIssue.Fields.Summary}[/]"));

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[blue]Issues with no estimation...[/]");
                    var nonEstimatedTickets = jiraIssues.Where(i => !i.Fields.IssueType.Name.Equals("Sub-task"))
                                                        .Where(i => i.Fields.Points == null || i.Fields.Points == 0)
                                                        .OrderBy(i => i.Fields.Reporter.DisplayName)
                                                        .ToList();

                    nonEstimatedTickets.ForEach(jiraIssue => AnsiConsole.MarkupLine($"[grey]~ {atlassianBaseUrl}/browse/{jiraIssue.Key} | {jiraIssue.Fields.Reporter.DisplayName ?? "Unassigned"} | {jiraIssue.Fields.IssueType.Name} | {jiraIssue.Fields.Summary}[/]"));

                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }

        private double? CalculateRemainingPoints(List<string> labels, double? originalEstimate)
        {
            var remainingPointsPrefix = "REM_";

            foreach (var label in labels)
            {
                //Override Original Estimation when the work has started
                Regex regex = new Regex($@"{remainingPointsPrefix}\d+");
                Match match = regex.Match(label.ToUpper());
                if (match.Success)
                {
                    var number = match.Value.Replace(remainingPointsPrefix, string.Empty);
                    return short.Parse(number);
                }
            }

            //If REM is not found return original estimate
            return originalEstimate;
        }

        private bool HasRemainingPoints(List<string> labels)
        {
            var remainingPointsPrefix = "REM_";

            foreach (var label in labels)
            {
                //Override Original Estimation when the work has started
                Regex regex = new Regex($@"{remainingPointsPrefix}\d+");
                Match match = regex.Match(label.ToUpper());
                if (match.Success)
                {
                    
                    return true;
                }
            }

            return false;
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
                                    .Where(j => !j.Fields.IssueType.Name.Equals("Sub-task"))
                                    .OrderBy(j => j.Fields.Reporter.DisplayName)
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
                          Assignee = current.Fields.Assignee?.DisplayName,
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
