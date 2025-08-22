using System.Text.Json;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage.Model;
using DevExLead.Core.Storage.Model.Jira;
using Spectre.Console;

namespace DevExLead.Core.Storage
{
    public static class UserStorageManager
    {
        private const string USER_CONFIGURATION_ID = "6b2e2731-a735-438e-bf6f-e749e0ebcd02";
        private static readonly string USER_CONFIGURATION_FILE = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Roaming\\Microsoft\\UserSecrets\\{USER_CONFIGURATION_ID}\\secrets.json";

        public static void Initialize()
        {
            UserStorage userStorage;

            if (!File.Exists(USER_CONFIGURATION_FILE))
            {
                userStorage = new UserStorage();
                SaveUserStorage(userStorage);
            }

            userStorage = GetUserStorage();

            if (userStorage.Vault == null)
            {
                userStorage.Vault = new Dictionary<string, string>();
            }

            if (userStorage.Commands == null)
            {
                userStorage.Commands = new List<Command>();
            }

            if (userStorage.Repositories == null)
            {
                userStorage.Repositories = new List<Repository>();
            }

            if (userStorage.EnvironmentVariables == null)
            {
                userStorage.EnvironmentVariables = new Dictionary<string, string>();
            }

            if (userStorage.Applications == null)
            {
                userStorage.Applications = new ApplicationsConfiguration();
                if (userStorage.Applications.Jira == null)
                {
                    userStorage.Applications.Jira = new JiraConfiguration();
                    if (userStorage.Applications.Jira.Templates == null)
                    {
                        userStorage.Applications.Jira.Templates = new List<JiraTemplate>();
                    }
                    if (userStorage.Applications.Jira.Users == null)
                    {
                        userStorage.Applications.Jira.Users = new List<JiraUser>();
                    }
                }
            }

            // Generate Encryption Keys if not present
            if (string.IsNullOrEmpty(userStorage.EncryptionKeys))
            {
                userStorage.EncryptionKeys = SecurityHelper.GenerateRSAKeys();
            }

            // Sort by name
            userStorage.Vault = userStorage.Vault.OrderBy(v => v.Key).ToDictionary();
            userStorage.Commands = userStorage.Commands.OrderBy(c => c.Key).ToList();
            userStorage.Repositories = userStorage.Repositories.OrderBy(c => c.Key).ToList();
            userStorage.EnvironmentVariables = userStorage.EnvironmentVariables.OrderBy(v => v.Key).ToDictionary();

            SaveUserStorage(userStorage);
        }

        public static UserStorage GetUserStorage()
        {
            var userConfigurationFileContent = StorageHelper.ReadFile(USER_CONFIGURATION_FILE);
            var userStorage = JsonSerializer.Deserialize<UserStorage>(userConfigurationFileContent);
            return userStorage;
        }

        public static string GetDecryptedValue(string key)
        {
            var userStorage = GetUserStorage();
            var encryptedValue = userStorage.Vault.FirstOrDefault(v => v.Key.Equals(key)).Value;
            if (encryptedValue == null)
            {
                var errorMessage = $"Error: {key} not found in Vault.";
                throw new Exception(errorMessage);
            }

            return SecurityHelper.DecryptVaultEntry(encryptedValue);
        }

        public static void SaveUserStorage(UserStorage userStorage)
        {
            var userStorageContent = JsonSerializer.Serialize(userStorage, new JsonSerializerOptions() { WriteIndented = true });
            var userStorageFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Roaming\\Microsoft\\UserSecrets\\{USER_CONFIGURATION_ID}\\secrets.json";
            StorageHelper.SaveFile(userStorageFile, userStorageContent);
        }
    }
}