using DevExLead.Integrations.JIRA.Model;

namespace DevExLead.Modules.Jira.Model
{
    internal class JiraIssueReestimate
    {
        public JiraIssueReestimate()
        {
        }

        public string Key { get; set; }
        public string Summary { get; set; }
        public string? Assignee { get; set; }
        public double? OldEstimate { get; set; }
        public double? NewEstimate { get; set; }
        public JiraIssueType IssueType { get; set; }
        
    }
}