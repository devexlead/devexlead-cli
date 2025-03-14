namespace DevExLead.Core.Storage
{
    public static class StorageHelper
    {
        public static string ReadFile(string fileLocation)
        {
            return File.ReadAllText(fileLocation);
        }

        public static void SaveFile(string fileLocation, string content)
        {
            File.WriteAllText(fileLocation, content);
        }
    }
}
