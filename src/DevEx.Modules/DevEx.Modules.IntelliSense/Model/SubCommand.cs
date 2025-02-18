namespace DevEx.Modules.IntelliSense.Model
{
    public class SubCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Handler { get; set; }
        public Parameter[] Parameters { get; set; }
    }
}