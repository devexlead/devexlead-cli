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
                else
                {
                    ShowSprintMetrics(jiraIssues);
                    ShowSprintBacklog(atlassianBaseUrl, jiraIssues);
                    ShowBacklogChanges(atlassianBaseUrl, jiraIssues, filePath);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }

        private void ShowSprintMetrics(List<JiraIssue> jiraIssues)
        {
            var totalPoints = jiraIssues.Where(i => i.Fields.Points != null)
                                   .Sum(i => i.Fields.Points ?? 0);

            var remainingPoints = jiraIssues.Where(i => i.Fields.Points != null)
                                      .Sum(i => CalculateRemainingPoints(i));
           

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[blue]Total Points: {totalPoints}[/]");
            AnsiConsole.MarkupLine($"[blue]Remaining Points: {remainingPoints}[/]");
        }

        private static void ShowSprintBacklog(string atlassianBaseUrl, List<JiraIssue> jiraIssues)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[blue]Sprint Backlog[/]");
            ShowTable(atlassianBaseUrl, jiraIssues);
        }

        // Replace the problematic code block where 'AddRow' is used with the correct method for adding rows to a Table.
        private static void ShowTable(string atlassianBaseUrl, List<JiraIssue> jiraIssues)
        {
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);


            table.AddColumn(new TableColumn("Epic"));
            table.AddColumn(new TableColumn("Type"));
            table.AddColumn(new TableColumn("Status"));
            table.AddColumn(new TableColumn("Summary"));

            table.AddColumn(new TableColumn("Points"));
            table.AddColumn(new TableColumn("Remaining"));

            table.AddColumn(new TableColumn("Assignee"));
            table.AddColumn(new TableColumn("Reporter"));

            foreach (var jiraIssue in jiraIssues.Where(i => !i.Fields.IssueType.Name.Equals("Sub-task")).OrderBy(i => i.Fields.Parent?.Key))
            {
                table.AddRow(
                    GetEpicMarkup(atlassianBaseUrl, jiraIssue.Fields.Parent),
                    GetIssueTypeMarkup(jiraIssue.Fields.IssueType.Name),
                    GetStatusMarkup(jiraIssue.Fields.Status.Name),
                    $"[link={atlassianBaseUrl}/browse/{jiraIssue.Key}]{jiraIssue.Key} | {jiraIssue.Fields.Summary}[/]",
                    GetPointsMarkup(jiraIssue.Fields.Points),
                    GetRemainingPointsMarkup(CalculateRemainingPoints(jiraIssue)),
                    GetAssigneeMarkup(jiraIssue.Fields.Assignee),
                    GetAssigneeMarkup(jiraIssue.Fields.Reporter)
                );
            }

            AnsiConsole.Write(table);
        }

        private static string GetRemainingPointsMarkup(double? remaining)
        {
            if (remaining.HasValue && remaining.Value > 0)
                return $"[red]{remaining.Value}[/]";
            return "[grey]0[/]";
        }

        private static string GetEpicMarkup(string atlassianBaseUrl, JiraParent parent)
        {
            if (parent == null || string.IsNullOrWhiteSpace(parent.Key))
                return "[red]No Epic[/]";
            return $"[link={atlassianBaseUrl}/browse/{parent.Key}][grey]{parent.Key} | {parent.Fields.Summary}[/][/]";
        }


        private static string GetIssueTypeMarkup(string issueTypeName)
        {
            return issueTypeName.ToLower() switch
            {
                "bug" => "[red]Bug[/]",
                "story" => "[green]Story[/]",
                "task" => "[blue]Task[/]",
                "spike" => "[orange1]Spike[/]",
                _ => $"[grey]{issueTypeName}[/]"
            };
        }


        private static void ShowBacklogChanges(string atlassianBaseUrl, List<JiraIssue> jiraIssues, string filePath)
        {
            AnsiConsole.WriteLine();

            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]No Snapshot found at {filePath}[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]Read {jiraIssues.Count} issues from {filePath}[/]");
            var json = File.ReadAllText(filePath);

            var plannedIssues = JsonSerializer.Deserialize<List<JiraIssue>>(json);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[blue]Added to Sprint...[/]");
            var addedIssues = FindAddedIssues(jiraIssues, plannedIssues);
            ShowTable(atlassianBaseUrl, addedIssues);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[blue]Removed from Sprint...[/]");
            var deletedIssues = FindDeletedIssues(jiraIssues, plannedIssues);
            ShowTable(atlassianBaseUrl, deletedIssues);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[blue]Re-Estimated during Sprint...[/]");
            var reestimatedIssues = FindReestimatedIssues(jiraIssues, plannedIssues);
            ShowTable(atlassianBaseUrl, reestimatedIssues.Select(i => i.Issue).ToList());
        }

        private static string GetPointsMarkup(double? points)
        {
            if (points == null || points == 0)
                return "[red]0[/]";
            return $"[green]{points}[/]";
        }

        private static string GetAssigneeMarkup(JiraUser assignee)
        {
            if (assignee == null || string.IsNullOrWhiteSpace(assignee.DisplayName))
                return "[red]Unassigned[/]";
            return $"[grey]{assignee.DisplayName}[/]";
        }


        /// <summary>
        /// TODO: Make this configurable
        /// </summary>
        /// <param name="statusName"></param>
        /// <returns></returns>
        private static string GetStatusMarkup(string statusName)
        {
            return statusName.ToLower() switch
            {
                "done" or "ready to release" => $"[green]{statusName}[/]",
                "in progress" or "testing" or "code review" or "design review" or "qa" or "uat" => $"[yellow]{statusName}[/]",
                "todo" or "blocked" or "not doing" or "on hold" or "duplicate" => $"[red]{statusName}[/]",
                _ => $"[red]{statusName}[/]"
            };
        }


        private static double? CalculateRemainingPoints(JiraIssue issue)
        {
            var remainingPointsPrefix = "REM_";

            foreach (var label in issue.Fields.Labels)
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

            if (issue.Fields.Status.Name.Equals("Done") ||
                issue.Fields.Status.Name.Equals("Not Doing") ||
                issue.Fields.Status.Name.Equals("Duplicate"))
            {
                return 0;
            }
            else
            {
                //If REM is not found return original estimate
                return issue.Fields.Points;
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
                          Issue = current,
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
