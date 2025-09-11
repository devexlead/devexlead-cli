namespace DevExLead.Core.Model.Command
{
    public class DxcParameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
    }
}