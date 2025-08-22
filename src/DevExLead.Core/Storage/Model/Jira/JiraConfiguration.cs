namespace DevExLead.Core.Storage.Model.Jira
{
    public class JiraConfiguration
    {
        public string BaseUrl { get; set; }
        public int BoardId { get; set; }
        public List<string> IssueTypes { get; set; }
        public List<string> Priorities { get; set; }
        public List<JiraTemplate> Templates { get; set; }
        public List<JiraUser> Users { get; set; }

    }
}