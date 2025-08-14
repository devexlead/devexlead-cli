using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Core.Storage.Model;
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
                bool isVerbose = ParameterHelper.ReadBoolParameter(options, "isVerbose");
                var jiraConnector = JiraHelper.GetJiraConnector(isVerbose, out string atlassianBaseUrl);

                var request = new JiraIssueCreateRequest
                {
                    Fields = new JiraFieldsCreateRequest
                    {
                        Summary = AnsiConsole.Ask<string>("Enter summary:"),
                        IssueType = new JiraIssueType { Name = JiraHelper.SelectIssueType() },
                        Priority = new JiraPriority { Name = JiraHelper.SelectPriority() },
                    }
                };

                ApplyTemplateToRequest(request);

                ApplySprintToRequest(jiraConnector, request);

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

        private static void ApplySprintToRequest(Integrations.JIRA.JiraConnector? jiraConnector, JiraIssueCreateRequest request)
        {
            var selectedSprint = JiraHelper.SelectSprint(jiraConnector);

            var jiraSprint = selectedSprint?.Id > 0 ? selectedSprint : null;

            if (request.Fields.IssueType.Name.Equals(IssueTypeConstants.STORY) ||
                request.Fields.IssueType.Name.Equals(IssueTypeConstants.BUG) ||
                request.Fields.IssueType.Name.Equals(IssueTypeConstants.TASK))
            {
                request.Fields.SprintId = jiraSprint?.Id;
            }
        }

        private static void ApplyTemplateToRequest(JiraIssueCreateRequest request)
        {
            var templates = UserStorageManager.GetUserStorage().JiraTemplates;
            //show templates in a list with AnsiConsole, choose by name and return JiraTemplate object
            var selectedTemplate = AnsiConsole.Prompt(
                new SelectionPrompt<JiraTemplate>()
                    .Title("Select a template:")
                    .UseConverter(t => t.Name)
                    .AddChoices(templates)
            );

            request.Fields.Project = new JiraProject { Key = selectedTemplate.ProjectKey };
            request.Fields.Parent = new JiraParent { Key = selectedTemplate.ParentKey };
        }
    }
}
