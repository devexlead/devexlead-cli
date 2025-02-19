using System.Text.Json;
using DevEx.Core.Helpers;
using DevEx.Core.Storage.Model;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace DevEx.Core.Storage
{
    public static class UserStorageManager
    {
        private const string USER_CONFIGURATION_ID = "6b2e2731-a735-438e-bf6f-e749e0ebcd02";

        private static IConfigurationRoot? _configuration;
        public static IConfigurationRoot Configuration
        {
            get
            {
                return _configuration;
            }
        }

        public static void Initialize()
        {
            _configuration = new ConfigurationBuilder()
                                    .SetBasePath(AppContext.BaseDirectory)
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                    .AddUserSecrets(USER_CONFIGURATION_ID, reloadOnChange: true)
                                    .Build();

            var userStorage = GetUserStorage();

            if (userStorage.Vault == null)
            {
                userStorage.Vault = new Dictionary<string, string>();
            }

            if (userStorage.Applications == null)
            {
                userStorage.Applications = new List<Application>();
            }

            if (userStorage.Bookmarks == null)
            {
                userStorage.Bookmarks = new List<string>();
            }

            SaveUserStorage(userStorage);
        }

        public static UserStorage GetUserStorage()
        {
            var userConfigurationFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Roaming\\Microsoft\\UserSecrets\\{USER_CONFIGURATION_ID}\\secrets.json";
            var userConfigurationFileContent = StorageHelper.ReadFile(userConfigurationFile);
            var userStorage = JsonSerializer.Deserialize<UserStorage>(userConfigurationFileContent);
            return userStorage;
        }

        public static string GetDecryptedValue(string key)
        {
            var userStorage = GetUserStorage();
            var encryptedValue = userStorage.Vault.FirstOrDefault(v => v.Key.Equals(key)).Value;
            if (encryptedValue == null)
            {
                var errorMessage = $"Error: {key} not found in user storage.";
                AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");
                throw new ArgumentException(errorMessage);
            }
            return EncryptionHelper.Decrypt(encryptedValue);
        }

        public static void SaveUserStorage(UserStorage userStorage)
        {
            var userStorageContent = JsonSerializer.Serialize(userStorage, new JsonSerializerOptions() { WriteIndented = true });
            var userStorageFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Roaming\\Microsoft\\UserSecrets\\{USER_CONFIGURATION_ID}\\secrets.json";
            StorageHelper.SaveFile(userStorageFile, userStorageContent);
            _configuration.Reload();
        }
    }
}