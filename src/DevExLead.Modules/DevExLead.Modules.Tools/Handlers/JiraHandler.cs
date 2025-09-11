using System.Data;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Modules.Jira.Helpers;
using DevExLead.Integrations.JIRA.Model;

namespace DevExLead.Modules.Export.Handlers
{
    public class JiraHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var path = ParameterHelper.ReadStringParameter(options, "path");
            var format = ParameterHelper.ReadStringParameter(options, "format");
            var jql = ParameterHelper.ReadStringParameter(options, "jql");

            bool isLoggingEnabled = UserStorageManager.GetUserStorage().IsLoggingEnabled;
            var jiraConnector = JiraHelper.GetJiraConnector(isLoggingEnabled, out string atlassianBaseUrl);

            var jiraIssues = await jiraConnector.RunJqlAsync(jql);

            // Convert jiraIssues to DataTable
            var dataTable = ConvertJiraIssuesToDataTable(jiraIssues);


            var fileName = "jira";
            switch (format)
            {
                case "json":
                    FileHelper.ExportAsJsonFile(dataTable, path, fileName);
                    break;
                case "csv":
                    FileHelper.ExportAsCsvFile(dataTable, path, fileName);
                    break;
                case "xls":
                    await FileHelper.ExportAsExcelFile(dataTable, path, fileName);
                    break;
                default:
                    FileHelper.ExportAsCsvFile(dataTable, path, fileName);
                    break;
            }
        }

        private static DataTable ConvertJiraIssuesToDataTable(List<JiraIssue> jiraIssues)
        {
            var dataTable = new DataTable("JiraIssues");

            // Add columns
            dataTable.Columns.Add("Key", typeof(string));
            dataTable.Columns.Add("Summary", typeof(string));
            dataTable.Columns.Add("Status", typeof(string));
            dataTable.Columns.Add("IssueType", typeof(string));
            dataTable.Columns.Add("Priority", typeof(string));
            dataTable.Columns.Add("Points", typeof(string));
            dataTable.Columns.Add("Assignee", typeof(string));
            dataTable.Columns.Add("Reporter", typeof(string));
            dataTable.Columns.Add("Epic", typeof(string));
            dataTable.Columns.Add("EpicName", typeof(string));

            // Add rows
            foreach (var issue in jiraIssues)
            {
                var row = dataTable.NewRow();
                row["Key"] = (object?)issue.Key ?? DBNull.Value;
                row["Summary"] = (object?)issue.Fields?.Summary ?? DBNull.Value;
                row["Status"] = (object?)issue.Fields?.Status?.Name ?? DBNull.Value;
                row["IssueType"] = (object?)issue.Fields?.IssueType?.Name ?? DBNull.Value;
                row["Priority"] = (object?)issue.Fields?.Priority?.Name ?? DBNull.Value;
                row["Points"] = (object?)issue.Fields?.Points ?? DBNull.Value;
                row["Assignee"] = (object?)issue.Fields?.Assignee?.DisplayName ?? DBNull.Value;
                row["Reporter"] = (object?)issue.Fields?.Reporter?.DisplayName ?? DBNull.Value;
                row["Epic"] = (object?)issue.Fields?.Parent?.Key ?? DBNull.Value;
                row["EpicName"] = (object?)issue.Fields?.Parent?.Fields.Summary ?? DBNull.Value;

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
