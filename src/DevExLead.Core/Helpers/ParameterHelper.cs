namespace DevExLead.Core.Helpers
{
    public class ParameterHelper
    {
        public static bool ReadBoolParameter(Dictionary<string, object> options, string paramName)
        {
            bool param = false;

            if (options.ContainsKey(paramName) && options[paramName]!=null)
            {
                param = (bool) options[paramName];
            }

            return param;
        }

        public static string ReadStringParameter(Dictionary<string, object> options, string paramName)
        {
            string param = string.Empty;

            if (options.ContainsKey(paramName) && options[paramName] != null)
            {
                param = (string) options[paramName];
            }

            return param;
        }

    }
}
