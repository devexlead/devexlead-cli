using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Integrations.JIRA.Model;
using DevExLead.Modules.Jira.Helpers;
using DevExLead.Modules.Jira.Model;
using Spectre.Console;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DevExLead.Modules.Jira.Handlers
{
    public class JiraProgressHandler : ICommandHandler
    {
        JiraConnector? _jiraConnector;

        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var isSnapshot = ParameterHelper.ReadBoolParameter(options, "isSnapshot");

            try
            {
                bool isLoggingEnabled = UserStorageManager.GetUserStorage().IsLoggingEnabled;
                var appFolder = UserStorageManager.GetUserStorage().AppDataFolder;

                _jiraConnector = JiraHelper.GetJiraConnector(isLoggingEnabled, out string atlassianBaseUrl);
                var selectedSprint = JiraHelper.SelectSprint(_jiraConnector);

                var jiraWatchJql = $"sprint = {selectedSprint?.Id}";
                AnsiConsole.MarkupLine($"[blue]Query: {jiraWatchJql} [/]");
                var jiraIssues = _jiraConnector.RunJqlAsync(jiraWatchJql).Result;

        
                var filePath = Path.Combine(appFolder, $"jira-issues-{selectedSprint?.Id}.json");

                if (isSnapshot)
                {
                    SaveJsonFile(jiraIssues, filePath);
                }
                else
                {
                    ShowSprintBacklog(atlassianBaseUrl, jiraIssues);
                    ShowBacklogChanges(atlassianBaseUrl, jiraIssues, filePath);
                    await WatchJiraIssues(jiraIssues);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }

        private async Task WatchJiraIssues(List<JiraIssue> jiraIssues)
        {
            foreach (var jiraIssue in jiraIssues)
            {
                //AnsiConsole.MarkupLine($"[green]Watching issue {jiraIssue.Key} for {jiraIssue.Fields.Reporter?.DisplayName} and {jiraIssue.Fields.Assignee?.DisplayName}[/]");

                if (jiraIssue.Fields.Reporter != null)
                {
                    await _jiraConnector.WatchIssueWithAccountIdAsync(jiraIssue.Key, jiraIssue.Fields.Reporter.AccountId);
                }

                if (jiraIssue.Fields.Assignee != null)
                {
                    await _jiraConnector.WatchIssueWithAccountIdAsync(jiraIssue.Key, jiraIssue.Fields.Assignee.AccountId);
                }

                var watchers = UserStorageManager.GetUserStorage().Applications.Jira.Users.Where(u => u.IsWatcher);
                foreach (var watcher in watchers)
                {
                    if (watcher.Id != null)
                    {
                        await _jiraConnector.WatchIssueWithAccountIdAsync(jiraIssue.Key, watcher.Id);
                    }
                }
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

        private static void ShowRemainingPointsPerEpic(string atlassianBaseUrl, List<JiraIssue> jiraIssues)
        {
            var epics = jiraIssues
                .Where(i => !i.Fields.IssueType.Name.Equals("Sub-task"))
                .GroupBy(i => i.Fields.Parent?.Key)
                .OrderBy(g => g.FirstOrDefault()?.Fields.Parent?.Fields.Summary ?? "No Epic");

            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn("Epic");
            table.AddColumn("Total Points");
            table.AddColumn("Remaining Points");

            foreach (var epic in epics)
            {
                var epicKey = epic.Key;
                var total = epic.Sum(i => i.Fields.Points) ?? 0;
                var remaining = epic.Sum(i => CalculateRemainingPoints(i)) ?? 0;

                string epicMarkup = GetEpicMarkup(atlassianBaseUrl, epic.FirstOrDefault()?.Fields.Parent);

                string totalMarkup = total > 0
                   ? $"[grey]{total}[/]"
                   : "[red]0[/]";

                string remainingMarkup = remaining > 0
                    ? $"[red]{remaining}[/]"
                    : "[green]0[/]";

                table.AddRow(epicMarkup, totalMarkup, remainingMarkup);
            }

            AnsiConsole.MarkupLine("[blue]Remaining Points per Epic[/]");
            AnsiConsole.Write(table);
        }



        private static void ShowPerInvestmentCategory(string atlassianBaseUrl, List<JiraIssue> jiraIssues)
        {
            var investmentProfile = UserStorageManager.GetUserStorage().InvestmentProfile;

            // Category -> Total Story Points
            var pointsCalculator = new Dictionary<string, double>() { { "Unassigned", 0d } };
            // Category -> Total Allocation Hours
            var hoursCalculator = new Dictionary<string, double>() { { "Unassigned", 0d } };

            // Calculate story points from Jira issues
            var topLevelIssues = jiraIssues
                .Where(i => !i.Fields.IssueType.Name.Equals("Sub-task"))
                .Where(i => i.Fields.Points is not null);

            foreach (var jiraIssue in topLevelIssues)
            {
                // Find the investment category by Epic key (issue.Fields.Parent is the Epic for a Story/Task/Bug)
                var epicKey = jiraIssue.Fields.Parent?.Key;
                var ic = epicKey == null
                    ? null
                    : investmentProfile.InvestmentCategories
                        .FirstOrDefault(c => c.Epics.Contains(epicKey));

                var points = jiraIssue.Fields.Points ?? 0;

                if (ic != null)
                {
                    if (!pointsCalculator.ContainsKey(ic.Name))
                        pointsCalculator[ic.Name] = 0;
                    pointsCalculator[ic.Name] += points;
                }
                else
                {
                    pointsCalculator["Unassigned"] += points;
                }
            }

            // Calculate hours from developer allocations
            if (investmentProfile.DeveloperAllocations != null && investmentProfile.DeveloperAllocations.Any())
            {
                foreach (var allocation in investmentProfile.DeveloperAllocations)
                {
                    var categoryName = allocation.InvestmentCategory ?? "Unassigned";
                    
                    if (!hoursCalculator.ContainsKey(categoryName))
                        hoursCalculator[categoryName] = 0;
                    
                    hoursCalculator[categoryName] += allocation.Hours;
                }
            }

            // Get all unique categories from both calculations
            var allCategories = pointsCalculator.Keys.Union(hoursCalculator.Keys).ToList();

            var totalPoints = pointsCalculator.Values.Sum();
            var totalHours = hoursCalculator.Values.Sum();

            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn("Investment Category");
            table.AddColumn("Points");
            table.AddColumn("% Points");
            table.AddColumn("Hours");
            table.AddColumn("% Hours");

            foreach (var category in allCategories
                     .OrderByDescending(k => k.Equals("Unassigned") ? -1 : Math.Max(pointsCalculator.GetValueOrDefault(k), hoursCalculator.GetValueOrDefault(k))))
            {
                var points = pointsCalculator.GetValueOrDefault(category);
                var hours = hoursCalculator.GetValueOrDefault(category);
                var pointsPct = totalPoints > 0 ? (points / totalPoints) * 100d : 0d;
                var hoursPct = totalHours > 0 ? (hours / totalHours) * 100d : 0d;

                string pointsMarkup = points > 0 ? $"[grey]{points}[/]" : "[red]0[/]";
                string hoursMarkup = hours > 0 ? $"[grey]{hours:0.##}h[/]" : "[red]0h[/]";
                
                string pointsPctMarkup = pointsPct switch
                {
                    > 25 => $"[green]{pointsPct:0.##}%[/]",
                    > 0 => $"[yellow]{pointsPct:0.##}%[/]",
                    _ => "[red]0%[/]"
                };

                string hoursPctMarkup = hoursPct switch
                {
                    > 25 => $"[green]{hoursPct:0.##}%[/]",
                    > 0 => $"[yellow]{hoursPct:0.##}%[/]",
                    _ => "[red]0%[/]"
                };

                table.AddRow(
                    category == "Unassigned" ? "[red]Unassigned[/]" : $"[blue]{category}[/]",
                    pointsMarkup,
                    pointsPctMarkup,
                    hoursMarkup,
                    hoursPctMarkup
                );
            }

            AnsiConsole.MarkupLine("[blue]Investment Allocation (Points & Hours)[/]");
            AnsiConsole.Write(table);
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
            table.AddColumn(new TableColumn("Priority"));
            table.AddColumn(new TableColumn("Status"));
            table.AddColumn(new TableColumn("Summary"));

            table.AddColumn(new TableColumn("Points"));
            table.AddColumn(new TableColumn("Remaining"));

            table.AddColumn(new TableColumn("Assignee"));
            table.AddColumn(new TableColumn("Reporter"));

            foreach (var jiraIssue in jiraIssues.Where(i => !i.Fields.IssueType.Name.Equals("Sub-task")).OrderBy(i => i.Fields.Parent?.Key))
            {
                try
                {
                    table.AddRow(
                                   GetEpicMarkup(atlassianBaseUrl, jiraIssue.Fields.Parent),
                                   GetIssueTypeMarkup(jiraIssue.Fields.IssueType.Name),
                                   GetPriorityMarkup(jiraIssue.Fields.Priority.Name),
                                   GetStatusMarkup(jiraIssue.Fields.Status.Name),
                                   $"[link={atlassianBaseUrl}/browse/{jiraIssue.Key}][grey]{jiraIssue.Key} | {TruncateWithEllipsis(jiraIssue.Fields.Summary, 40)}[/][/]",
                                   GetPointsMarkup(jiraIssue.Fields.Points),
                                   GetRemainingPointsMarkup(CalculateRemainingPoints(jiraIssue)),
                                   GetAssigneeMarkup(jiraIssue.Fields.Assignee),
                                   GetAssigneeMarkup(jiraIssue.Fields.Reporter)
                    );
                }
                catch (Exception)
                {
                    AnsiConsole.MarkupLine($"[red]Error adding [link={atlassianBaseUrl}/browse/{jiraIssue.Key}]{jiraIssue.Key}[/] to the table[/]");
                }
            }

            AnsiConsole.Write(table);
        }

        private static string TruncateWithEllipsis(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            return text.Length > maxLength
                ? text.Substring(0, maxLength) + "..."
                : text;
        }


        private static string GetPriorityMarkup(string? priorityName)
        {
            if (string.IsNullOrWhiteSpace(priorityName))
                return "[grey]None[/]";
                return priorityName.ToLower() switch
                {
                    "high" or "highest" => "[red]High[/]",
                    "medium" => "[orange1]Medium[/]",
                    "low" or "lowest" => "[blue]Low[/]",
                    _ => $"[grey]{priorityName}[/]"
                };
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
            return $"[link={atlassianBaseUrl}/browse/{parent.Key}][grey]{parent.Key} | {TruncateWithEllipsis(parent.Fields.Summary, 300)}[/][/]";
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
            if (issue.Fields.Status.Name.Equals("Done") ||
                issue.Fields.Status.Name.Equals("Not Doing") ||
                issue.Fields.Status.Name.Equals("Duplicate") ||
                issue.Fields.Status.Name.Equals("Ready to Release"))
            {
                return 0;
            }
            else
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

                //If REM is not found return original estimate
                return issue.Fields.Points;
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
