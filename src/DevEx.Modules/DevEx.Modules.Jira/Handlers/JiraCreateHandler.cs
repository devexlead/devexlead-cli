using DevEx.Core;
using DevEx.Core.Storage;
using DevEx.Integrations.JIRA;
using DevEx.Integrations.JIRA.Model;
using DevEx.Integrations.JIRA.Model.Request;
using devex_integrations.JIRA.Constants;
using Spectre.Console;

namespace DevEx.Modules.Jira.Handlers
{
    public class JiraCreateHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
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

                var atlassianBaseUrl = UserStorageManager.GetDecryptedValue("AtlassianBaseUrl");
                if (atlassianBaseUrl == null) return;

                var atlassianUser = UserStorageManager.GetDecryptedValue("AtlassianUser");
                if (atlassianUser == null) return;

                var atlassianKey = UserStorageManager.GetDecryptedValue("AtlassianKey");
                if (atlassianKey == null) return;

                var jiraConnector = new JiraConnector(atlassianBaseUrl, atlassianUser, atlassianKey, isVerbose);

                var request = new JiraIssueCreateRequest
                {
                    Fields = new JiraFieldsCreateRequest
                    {
                        Project = new JiraProject { Key = AnsiConsole.Ask<string>("Enter project ID:") },
                        Summary = AnsiConsole.Ask<string>("Enter summary:"),
                        Description = AnsiConsole.Ask<string>("Enter description:"),
                        IssueType = new JiraIssueType { Name = SelectIssueType() },
                        Priority = new JiraPriority { Name = SelectPriority() },
                    }
                };

                if (request.Fields.IssueType.Name.Equals(IssueTypeConstants.STORY) ||
                    request.Fields.IssueType.Name.Equals(IssueTypeConstants.BUG) ||
                    request.Fields.IssueType.Name.Equals(IssueTypeConstants.TASK))
                {
                    request.Fields.Parent = SelectParent(jiraConnector, request.Fields.Project.Key, [IssueTypeConstants.EPIC]);
                    request.Fields.SprintId = SelectSprint(jiraConnector);
                }

                if (request.Fields.IssueType.Name.Equals(IssueTypeConstants.SUBTASK))
                {
                    request.Fields.Parent = SelectParent(jiraConnector, request.Fields.Project.Key, [IssueTypeConstants.STORY, IssueTypeConstants.BUG, IssueTypeConstants.TASK]);
                }

                if (request.Fields.IssueType.Name == IssueTypeConstants.EPIC)
                {
                    request.Fields.StartDate = AnsiConsole.Ask<DateOnly?>("Enter start date (optional, format: yyyy-MM-dd):", null);
                    request.Fields.DueDate = AnsiConsole.Ask<DateOnly?>("Enter due date (optional, format: yyyy-MM-dd):", null);
                }

                request.Fields.Assignee = SelectAssignee(jiraConnector);
                
                var result = jiraConnector.CreateIssueAsync(request).Result;

                AnsiConsole.MarkupLine($"[green]{atlassianBaseUrl}/browse/{result.Key}[/]");

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }

        private static long? SelectSprint(JiraConnector jiraConnector)
        {
            var atlassianTeamBoardId = UserStorageManager.GetDecryptedValue("AtlassianTeamBoardId");
            if (atlassianTeamBoardId == null) return null;

            var sprints = jiraConnector.FetchSprints(int.Parse(atlassianTeamBoardId)).Result;
            var sprintOptions = sprints.Select(s => s.Name).ToList();
            sprintOptions.Insert(0, "None");

            var selectedSprintName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a sprint (optional):")
                    .AddChoices(sprintOptions)
            );

            long? selectedSprintId = null;
            if (selectedSprintName != "None")
            {
                selectedSprintId = sprints.First(s => s.Name == selectedSprintName).Id;
            }
            return selectedSprintId;
        }

        private static string SelectPriority()
        {
            var priorityOptions = new List<string>
            {
                PriorityConstants.HIGHEST,
                PriorityConstants.HIGH,
                PriorityConstants.MEDIUM,
                PriorityConstants.LOW,
                PriorityConstants.LOWEST
            };

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a priority:")
                    .AddChoices(priorityOptions)
            );
        }

        private static string SelectIssueType()
        {
            var issueTypeOptions = new List<string>
            {
                IssueTypeConstants.EPIC,
                IssueTypeConstants.STORY,
                IssueTypeConstants.BUG,
                IssueTypeConstants.TASK,
                IssueTypeConstants.SUBTASK
            };

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an issue type:")
                    .AddChoices(issueTypeOptions)
            );
        }

        private static JiraUser? SelectAssignee(JiraConnector jiraConnector)
        {
            var atlassianUserGroupName = UserStorageManager.GetDecryptedValue("AtlassianUserGroupName");
            if (atlassianUserGroupName == null) return null;

            var jiraGroupMembersResponse = jiraConnector.GetJiraUsersByGroupName(atlassianUserGroupName).Result;
            var userOptions = jiraGroupMembersResponse.Users
                .Select(u => new { u.DisplayName, u.AccountId })
                .ToList();
            userOptions.Insert(0, new { DisplayName = "None", AccountId = (string)null });

            var selectedUser = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an assignee (optional):")
                    .AddChoices(userOptions.Select(u => $"{u.DisplayName} ({u.AccountId})").ToList())
            );

            if (selectedUser == "None (null)")
            {
                return null;
            }

            var selectedAccountId = userOptions.First(u => $"{u.DisplayName} ({u.AccountId})" == selectedUser).AccountId;
            return new JiraUser { AccountId = selectedAccountId };
        }

        private static JiraParent SelectParent(JiraConnector jiraConnector, string projectKey, List<string> parentIssueTypes)
        {
            var jql = $"project={projectKey} AND ({string.Join(" OR ", parentIssueTypes.Select(type => $"issuetype={type}"))})";
            var issues = jiraConnector.RunJqlAsync(jql).Result;
            var parentOptions = issues.Select(i => $"{i.Key} - {i.Fields.Summary}").ToList();
            parentOptions.Insert(0, "None");

            var selectedParent = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a parent issue (optional):")
                    .AddChoices(parentOptions)
            );

            if (selectedParent == "None")
            {
                return null;
            }

            var selectedParentKey = selectedParent.Split(" - ")[0];
            return new JiraParent { Key = selectedParentKey };
        }
    }
}
