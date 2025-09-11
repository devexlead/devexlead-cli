namespace DevExLead.Core.Model.Command
{
    public class DxcParameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }
        public bool IsRequired { get; set; }
    }
}