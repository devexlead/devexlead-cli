namespace DevEx.Core.Model.Command
{
    public class CommandItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SubCommandItem> SubCommands { get; set; }
    }
}
