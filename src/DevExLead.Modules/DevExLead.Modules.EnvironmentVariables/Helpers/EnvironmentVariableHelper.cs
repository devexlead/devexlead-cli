using DevEx.Core.Storage;

namespace DevEx.Modules.EnvironmentVariables.Helpers
{
    public static class EnvironmentVariableHelper
    {
        public static Dictionary<string, string> List()
        {
            return UserStorageManager.GetUserStorage().EnvironmentVariables;
        }

        public static void Add(string key, string value)
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

        public static void Delete(string key)
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
