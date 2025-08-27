using DevExLead.Core.Storage.Model.Jira;
using DevExLead.Core.Storage.Model.SqlServer;

namespace DevExLead.Core.Storage.Model
{
    public class ApplicationsConfiguration
    {
        public JiraConfiguration Jira { get; set; }
        public SqlServerConfiguration SqlServer { get; set; }
    }
}