namespace DevExLead.Core.Helpers
{
    public class ParameterHelper
    {
        public static bool ReadBoolParameter(Dictionary<string, string> options, string paramName)
        {
            bool param = false;

            if (options.ContainsKey(paramName) && !string.IsNullOrEmpty(options[paramName]))
            {
                param = bool.Parse(options[paramName]);
            }

            return param;
        }

        public static string ReadStringParameter(Dictionary<string, string> options, string paramName)
        {
            if (options.ContainsKey(paramName) && !string.IsNullOrEmpty(options[paramName]))
            {
                return options[paramName];
            }
            return null;
        }

    }
}
