namespace DevEx.Core.Storage.Model
{
    public class Application
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string RestoreCommand { get; set; }
        public string BuildCommand { get; set; }
        public string TestCommand { get; set; }
        public string RunCommand { get; set; }

    }
}