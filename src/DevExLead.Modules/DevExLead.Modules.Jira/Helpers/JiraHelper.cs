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
            atlassianBaseUrl = UserStorageManager.GetUserStorage().Applications.Jira.BaseUrl;
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
            var atlassianTeamBoardId = UserStorageManager.GetUserStorage().Applications.Jira.BoardId;
            if (atlassianTeamBoardId == null) return null;

            var jiraSprints = jiraConnector.FetchSprints(atlassianTeamBoardId)
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
            var priorityOptions = UserStorageManager.GetUserStorage().Applications.Jira.Priorities;

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a priority:")
                    .AddChoices(priorityOptions)
            );
        }

        public static string SelectIssueType()
        {
           var issueTypeOptions = UserStorageManager.GetUserStorage().Applications.Jira.IssueTypes;

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an issue type:")
                    .AddChoices(issueTypeOptions)
            );
        }

        public static JiraUser? SelectAssignee(JiraConnector jiraConnector)
        {
            var users = UserStorageManager.GetUserStorage().Applications.Jira.Users;

            if (users == null || users.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No users found in Jira configuration.[/]");
                return null;
            }

            // Add "Unassigned" option
            var unassignedString = "-";
            var unassigned = new Core.Storage.Model.Jira.JiraUser
            {
                Name = unassignedString,
                Id = string.Empty // or null, depending on your logic
            };

            var userList = users.ToList();
            userList.Insert(0, unassigned);

            var selectedUser = AnsiConsole.Prompt(
                new SelectionPrompt<Core.Storage.Model.Jira.JiraUser>()
                    .Title("Select an assignee:")
                    .UseConverter(u => u.Name == unassignedString ? unassignedString : $"{u.Name} ({u.Id})")
                    .AddChoices(userList)
            );

            // If "Unassigned" is selected, return null
            if (selectedUser.Name == unassignedString)
                return null;

            return new JiraUser { AccountId = selectedUser.Id };
        }
    }
}
