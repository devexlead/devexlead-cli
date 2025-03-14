namespace DevEx.Core.Storage.Model
{
    public class Repository
    {
        public string Name { get; set; }
        public string RemoteLocation { get; set; }
        public string WorkingFolder { get; set; }
        public string DefaultBranch { get; set; }
    }
}
