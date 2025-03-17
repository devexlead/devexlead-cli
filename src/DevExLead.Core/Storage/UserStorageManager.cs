using System.Security.Cryptography;
using System.Text.Json;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage.Model;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace DevExLead.Core.Storage
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

            // Generate Encryption Keys if not present
            if (string.IsNullOrEmpty(userStorage.EncryptionKeys))
            {
                // Generate the RSA Public (to encrypt) and Private (to decrypt) key pair
                using (RSA rsa = RSA.Create())
                {
                    //export both the public and private key information. This is necessary when you need
                    //to transfer the complete key pair for vault export/import purposes.
                    rsa.KeySize = 2048;
                    string keys = rsa.ToXmlString(true);

                    //Encrypt with machine specific algorithm
                    userStorage.EncryptionKeys = SecurityHelper.EncryptKey(keys);
                }
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
                var errorMessage = $"Error: {key} not found in Vault.";
                AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");
                return string.Empty;
            }
            return SecurityHelper.DecryptVaultEntry(encryptedValue);
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