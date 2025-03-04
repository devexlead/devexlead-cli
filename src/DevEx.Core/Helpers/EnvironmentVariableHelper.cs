using DevEx.Core.Storage;

namespace DevEx.Core.Helpers
{
    public static class EnvironmentVariableHelper
    {
        public static void SetEnvironmentVariables()
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
    }
}
