using DevExLead.Integrations.JIRA.Model;

namespace DevExLead.Modules.Jira.Model
{
    internal class JiraIssueReestimate
    {
        public JiraIssueReestimate()
        {
        }

        public JiraIssue Issue { get; set; }
        public double? OldEstimate { get; set; }
        public double? NewEstimate { get; set; }
    }
}