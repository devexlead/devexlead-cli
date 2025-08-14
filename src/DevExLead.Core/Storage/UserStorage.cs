using DevExLead.Core.Storage.Model;

namespace DevExLead.Core.Storage
{
    public class UserStorage
    {
        public string EncryptionKeys { get; set; }
        public Dictionary<string, string> Vault { get; set; }

        public List<Command> Commands { get; set; }
        public List<Repository> Repositories { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public List<JiraTemplate> JiraTemplates { get; set; }
    }
}
