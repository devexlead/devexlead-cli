using DevEx.Core.Storage;

namespace DevEx.Core.Helpers
{
    public static class EnvironmentVariableHelper
    {
        public static void ConfigureMachineEnvironmentVariables()
        {
            var variables = UserStorageManager.GetUserStorage().EnvironmentVariables;

            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value, EnvironmentVariableTarget.Machine);
            }
        }

        public static Dictionary<string, string> GetEnvironmentVariables()
        {
            return UserStorageManager.GetUserStorage().EnvironmentVariables;
        }

        public static void AddEnvironmentVariable(string key, string value)
        {
            var userStorage = UserStorageManager.GetUserStorage();

            if (userStorage.EnvironmentVariables.ContainsKey(key))
            {
                userStorage.EnvironmentVariables[key] = value;
            }
            else
            {
                userStorage.EnvironmentVariables.Add(key, value);
            }

            UserStorageManager.SaveUserStorage(userStorage);
        }

        public static void DeleteEnvironmentVariable(string key)
        {
            var userStorage = UserStorageManager.GetUserStorage();

            if (userStorage.EnvironmentVariables.ContainsKey(key))
            {
                userStorage.EnvironmentVariables.Remove(key);
            }

            UserStorageManager.SaveUserStorage(userStorage);
        }
    }
}
