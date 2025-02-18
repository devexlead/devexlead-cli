namespace DevEx.Core.Model.Command
{
    public class SubCommandItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Handler { get; set; }
        public List<ParameterItem> Parameters { get; set; }
    }
}
