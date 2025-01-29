using System.Reflection;

namespace DevEx.Core
{
    public class Helper
    {
        public static string? GetCurrentVersion()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
        }

        //public static void UpdateAutoComplete()
        //{
        //    string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        //    string psReadLineFile = Path.Combine(userFolder, "AppData\\Roaming\\Microsoft\\Windows\\PowerShell\\PSReadLine\\ConsoleHost_history.txt");

        //    //Remove all HF CLI Commands
        //    string[] lines = File.ReadAllLines(psReadLineFile);
        //    string[] filteredLines = lines.Where(line => !line.Contains("engmgr")).ToArray();
        //    File.WriteAllLines(psReadLineFile, filteredLines);

        //    //Insert new HF CLI Commands
        //    var autoCompleteContent = File.ReadAllText($"{toolPath}\\Files\\AutoComplete.txt");
        //    File.AppendAllText(psReadLineFile, autoCompleteContent);
        //}
    }
}

