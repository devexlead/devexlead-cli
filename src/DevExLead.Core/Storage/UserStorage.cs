using DevExLead.Core.Storage.Model;
using DevExLead.Core.Storage.Model.Investment;

namespace DevExLead.Core.Storage
{
    public class UserStorage
    {
        public bool IsLoggingEnabled { get; set; }
        public string AppDataFolder { get; set; }
        public string EncryptionKeys { get; set; }
        public Dictionary<string, string> Vault { get; set; }

        public List<Command> Commands { get; set; }
        public List<Repository> Repositories { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public ApplicationsConfiguration Applications { get; set; }
        public InvestmentProfile InvestmentProfile { get; set; }
    }
}
