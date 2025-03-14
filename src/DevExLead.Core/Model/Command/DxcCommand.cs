namespace DevEx.Core.Model.Command
{
    public class DxcCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DxcSubCommand[] SubCommands { get; set; }
    }
}