namespace DevEx.Modules.IntelliSense.Model
{
    public class Command
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SubCommand[] SubCommands { get; set; }
    }
}