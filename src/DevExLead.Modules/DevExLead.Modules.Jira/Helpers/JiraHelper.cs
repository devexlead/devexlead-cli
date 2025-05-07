using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Integrations.JIRA.Model;
using Spectre.Console;

namespace DevExLead.Modules.Jira.Helpers
{
    public static class JiraHelper
    {
        public static long? SelectSprint(JiraConnector jiraConnector)
        {
            var atlassianTeamBoardId = UserStorageManager.GetDecryptedValue("Atlassian:BoardId");
            if (atlassianTeamBoardId == null) return null;

            var sprints = jiraConnector.FetchSprints(int.Parse(atlassianTeamBoardId))
                                       .Result.Where(s => s.State.Equals("active") ||
                                                          s.State.Equals("future"));
            var sprintOptions = sprints.Select(s => s.Name).ToList();
            sprintOptions.Insert(0, "None");

            var selectedSprintName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a sprint:")
                    .AddChoices(sprintOptions)
            );

            long? selectedSprintId = null;
            if (selectedSprintName != "None")
            {
                selectedSprintId = sprints.First(s => s.Name == selectedSprintName).Id;
            }
            return selectedSprintId;
        }

        public static string SelectPriority()
        {
            var priorityOptions = UserStorageManager.GetDecryptedValue("Atlassian:Jira:Priorities").Split("|");

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a priority:")
                    .AddChoices(priorityOptions)
            );
        }

        public static string SelectIssueType()
        {
           var issueTypeOptions = UserStorageManager.GetDecryptedValue("Atlassian:Jira:IssueTypes").Split("|");

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an issue type:")
                    .AddChoices(issueTypeOptions)
            );
        }

        public static JiraUser? SelectAssignee(JiraConnector jiraConnector)
        {
            var atlassianUserGroupName = UserStorageManager.GetDecryptedValue("Atlassian:Assignees");
            if (atlassianUserGroupName == null) return null;

            var jiraGroupMembersResponse = jiraConnector.GetJiraUsersByGroupName(atlassianUserGroupName).Result;
            var userOptions = jiraGroupMembersResponse.Users
                .Select(u => new { u.DisplayName, u.AccountId })
                .ToList();
            userOptions.Insert(0, new { DisplayName = "None", AccountId = (string)null });

            var selectedUser = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an assignee:")
                    .AddChoices(userOptions.Select(u => $"{u.DisplayName} ({u.AccountId})").ToList())
            );

            if (selectedUser == "None (null)")
            {
                return null;
            }

            var selectedAccountId = userOptions.First(u => $"{u.DisplayName} ({u.AccountId})" == selectedUser).AccountId;
            return new JiraUser { AccountId = selectedAccountId };
        }

        public static JiraParent SelectParent(JiraConnector jiraConnector, string projectKey, long? sprintId, List<string> parentIssueTypes)
        {
            var jql = $"project={projectKey} AND ({string.Join(" OR ", parentIssueTypes.Select(type => $"issuetype={type}"))})";
            if (sprintId is not null)
            {
                jql += $" AND sprint = {sprintId}";
            }

            var issues = jiraConnector.RunJqlAsync(jql).Result;
            var parentOptions = issues.Select(i => $"{i.Key} - {i.Fields.Summary}").ToList();
            parentOptions.Insert(0, "None");

            var selectedParent = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a parent issue:")
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
