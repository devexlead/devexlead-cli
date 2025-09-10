namespace DevExLead.Modules.Tools.Model
{
    public class DatadogSettingsFile
    {
        public Metadata Metadata { get; set; }
    }

    public class Metadata
    {
        public List<Link> Links { get; set; }
    }



    public class Link
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Provider { get; set; }
    }

   
}
