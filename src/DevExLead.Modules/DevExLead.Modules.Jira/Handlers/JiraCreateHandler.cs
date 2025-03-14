using DevEx.Core;
using DevEx.Core.Storage;
using DevEx.Integrations.JIRA;
using DevEx.Integrations.JIRA.Model;
using DevEx.Integrations.JIRA.Model.Request;
using DevEx.Modules.Jira.Helpers;
using devex_integrations.JIRA.Constants;
using Spectre.Console;

namespace DevEx.Modules.Jira.Handlers
{
    public class JiraCreateHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
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

                var atlassianBaseUrl = UserStorageManager.GetDecryptedValue("Atlassian:BaseUrl");
                if (atlassianBaseUrl == null) return;

                var atlassianUser = UserStorageManager.GetDecryptedValue("Atlassian:User");
                if (atlassianUser == null) return;

                var atlassianKey = UserStorageManager.GetDecryptedValue("Atlassian:Key");
                if (atlassianKey == null) return;

                var jiraConnector = new JiraConnector(atlassianBaseUrl, atlassianUser, atlassianKey, isVerbose);

                var request = new JiraIssueCreateRequest
                {
                    Fields = new JiraFieldsCreateRequest
                    {
                        Project = new JiraProject { Key = UserStorageManager.GetDecryptedValue("Atlassian:Jira:ProjectKey") },
                        Summary = AnsiConsole.Ask<string>("Enter summary:"),
                        Description = AnsiConsole.Ask<string>("Enter description:"),
                        IssueType = new JiraIssueType { Name = JiraHelper.SelectIssueType() },
                        Priority = new JiraPriority { Name = JiraHelper.SelectPriority() },
                    }
                };

                if (request.Fields.IssueType.Name.Equals(IssueTypeConstants.STORY) ||
                    request.Fields.IssueType.Name.Equals(IssueTypeConstants.BUG) ||
                    request.Fields.IssueType.Name.Equals(IssueTypeConstants.TASK))
                {
                    request.Fields.Parent = JiraHelper.SelectParent(jiraConnector, request.Fields.Project.Key, [IssueTypeConstants.EPIC]);
                    request.Fields.SprintId = JiraHelper.SelectSprint(jiraConnector);
                }

                if (request.Fields.IssueType.Name.Equals(IssueTypeConstants.SUBTASK))
                {
                    request.Fields.Parent = JiraHelper.SelectParent(jiraConnector, request.Fields.Project.Key, [IssueTypeConstants.STORY, IssueTypeConstants.BUG, IssueTypeConstants.TASK]);
                }

                if (request.Fields.IssueType.Name == IssueTypeConstants.EPIC)
                {
                    request.Fields.StartDate = AnsiConsole.Ask<DateOnly?>("Enter start date (format: yyyy-MM-dd):", null);
                    request.Fields.DueDate = AnsiConsole.Ask<DateOnly?>("Enter due date (format: yyyy-MM-dd):", null);
                }

                //TODO: Make this configurable in local config file
                //request.Fields.Assignee = JiraHelper.SelectAssignee(jiraConnector);

                var result = jiraConnector.CreateIssueAsync(request).Result;

                AnsiConsole.MarkupLine($"[green]{atlassianBaseUrl}/browse/{result.Key}[/]");

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex}[/]");
            }
        }
    }
}
