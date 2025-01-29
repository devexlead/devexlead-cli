namespace DevEx.Core.Storage
{
    public class UserStorage
    {
        public string Version { get; set; }
        public Dictionary<string, string> Vault { get; set; }
    }
}
