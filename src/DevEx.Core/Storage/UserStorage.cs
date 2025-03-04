using DevEx.Core.Storage.Model;

namespace DevEx.Core.Storage
{
    public class UserStorage
    {
        public string Version { get; set; }
        public Dictionary<string, string> Vault { get; set; }

        public List<Command> Commands { get; set; }
        public List<Repository> Repositories { get; set; }

        public List<string> Bookmarks { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
    }
}
