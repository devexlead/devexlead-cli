using DevExLead.Core.Storage;
using System.Text.RegularExpressions;

namespace DevExLead.Core.Helpers
{
    public static class VariableHelper
    {
        public static string ReplacePlaceholders(string input)
        {
            //Timeout to address: Not specifying a timeout for regular expressions is security-sensitive
            //https://sonarsource.github.io/rspec/#/rspec/S6444/csharp
            var timeout = TimeSpan.FromSeconds(5); 
            var regex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.None, timeout);

            return regex.Replace(input, match =>
            {
                var key = match.Groups[1].Value;
                var placeHolder = UserStorageManager.GetDecryptedValue(key) ?? match.Value;
                placeHolder = ReplacePlaceholders(placeHolder); // Recursively replace placeholders
                return placeHolder;
            });
        }
    }
}
