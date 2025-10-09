namespace DevExLead.Core.Storage.Model
{
    public class Repository
    {
        public string Key { get; set; }
        public string RemoteLocation { get; set; }
        public string WorkingFolder { get; set; }
        public string DefaultBranch { get; set; }
        public List<string> Pipelines { get; set; }
    }
}
