namespace DevExLead.Core.Storage.Model
{
    public class AzureDevOps
    {
        public string Organization { get; set; }
        public string Project { get; set; }
        public string PersonalAccessToken { get; set; }
        public List<string> StagesToSkip { get; set; }
    }
}