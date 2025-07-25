using DevExLead.Core;
using DevExLead.Core.Storage;
using DevExLead.Integrations.JIRA;
using DevExLead.Integrations.JIRA.Constants;
using DevExLead.Integrations.JIRA.Model;
using DevExLead.Integrations.JIRA.Model.Request;
using DevExLead.Modules.Jira.Helpers;
using Spectre.Console;

namespace DevExLead.Modules.Jira.Handlers
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
                        IssueType = new JiraIssueType { Name = JiraHelper.SelectIssueType() },
                        Priority = new JiraPriority { Name = JiraHelper.SelectPriority() },
                    }
                };

                var jiraSprint = JiraHelper.SelectSprint(jiraConnector);

                if (request.Fields.IssueType.Name.Equals(IssueTypeConstants.STORY) ||
                    request.Fields.IssueType.Name.Equals(IssueTypeConstants.BUG) ||
                    request.Fields.IssueType.Name.Equals(IssueTypeConstants.TASK))
                {
                    request.Fields.Parent = JiraHelper.SelectParent(jiraConnector, request.Fields.Project.Key, jiraSprint?.Id, [IssueTypeConstants.EPIC]);
                    request.Fields.SprintId = jiraSprint?.Id;
                }

                if (request.Fields.IssueType.Name.Equals(IssueTypeConstants.SUBTASK))
                {
                    request.Fields.Parent = JiraHelper.SelectParent(jiraConnector, request.Fields.Project.Key, jiraSprint?.Id, [IssueTypeConstants.STORY, IssueTypeConstants.BUG, IssueTypeConstants.TASK]);
                }

                //if (request.Fields.IssueType.Name == IssueTypeConstants.EPIC)
                //{
                //    request.Fields.StartDate = AnsiConsole.Ask<DateOnly?>("Enter start date (format: yyyy-MM-dd):", null);
                //    request.Fields.DueDate = AnsiConsole.Ask<DateOnly?>("Enter due date (format: yyyy-MM-dd):", null);
                //}

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
