namespace DevExLead.Core.Helpers
{
    public class ParameterHelper
    {
        public static bool ReadBoolParameter(Dictionary<string, object> options, string paramName)
        {
            bool param = false;

            if (options.ContainsKey(paramName) && options[paramName] != null)
            {
                var value = options[paramName];
                
                // Handle boolean values directly
                if (value is bool boolValue)
                {
                    param = boolValue;
                }
                // Handle string values that represent booleans
                else if (value is string stringValue)
                {
                    bool.TryParse(stringValue, out param);
                }
            }

            return param;
        }

        public static string ReadStringParameter(Dictionary<string, object> options, string paramName)
        {
            string param = string.Empty;

            if (options.ContainsKey(paramName) && options[paramName] != null)
            {
                param = options[paramName].ToString() ?? string.Empty;
            }

            return param;
        }
    }
}
