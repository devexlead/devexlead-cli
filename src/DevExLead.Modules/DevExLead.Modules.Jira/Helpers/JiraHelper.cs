using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Integrations.JIRA.Model;
using Spectre.Console;

namespace DevExLead.Modules.Jira.Helpers
{
    public static class JiraHelper
    {
        public static JiraConnector? GetJiraConnector(bool isVerbose, out string atlassianBaseUrl)
        {
            atlassianBaseUrl = UserStorageManager.GetDecryptedValue("Atlassian:BaseUrl");
            if (atlassianBaseUrl == null) return null;

            var atlassianUser = UserStorageManager.GetDecryptedValue("Atlassian:User");
            if (atlassianUser == null) return null;

            var atlassianKey = UserStorageManager.GetDecryptedValue("Atlassian:Key");
            if (atlassianKey == null) return null;

            var jiraConnector = new JiraConnector(atlassianBaseUrl, atlassianUser, atlassianKey, isVerbose);

            return jiraConnector;
        }

        public static JiraSprint? SelectSprint(JiraConnector jiraConnector)
        {
            var atlassianTeamBoardId = UserStorageManager.GetDecryptedValue("Atlassian:BoardId");
            if (atlassianTeamBoardId == null) return null;

            var jiraSprints = jiraConnector.FetchSprints(int.Parse(atlassianTeamBoardId))
                                       .Result.Where(s => s.State.Equals("active") ||
                                                          s.State.Equals("future")).ToList();

            jiraSprints.Insert(0, new JiraSprint { Id = 0, Name = "-" }); // Add option for No Sprint

            // Let user select a single sprint
            var selectedSprint = AnsiConsole.Prompt(
                new SelectionPrompt<JiraSprint>()
                    .Title("Select Sprint:")
                    .UseConverter(s => $"{s.Name}")
                    .AddChoices(jiraSprints)
            );

            return selectedSprint;
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

        
    }
}
