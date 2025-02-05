using System.Text.Json;
using DevEx.Modules.IntelliSense.Model;

namespace DevEx.Modules.IntelliSense.Helpers
{
    internal class IntelliSenseHelper
    {
        /// <summary>
        /// Reads the JSON configuration from a file and returns a list of command lines,
        /// one for each command and sub-command combination, with explicit parameter names.
        /// </summary>
        /// <param name="filePath">The path to the JSON configuration file.</param>
        /// <returns>A list of strings representing the command lines.</returns>
        public static List<string> GetCommandLinesFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The JSON configuration file was not found.", filePath);
            }

            // Read the JSON content from the file.
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON into our classes.
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var root = JsonSerializer.Deserialize<Root>(json, options);

            var commandLines = new List<string>();

            if (root?.Commands != null)
            {
                foreach (var command in root.Commands)
                {
                    if (command.SubCommands != null)
                    {
                        foreach (var sub in command.SubCommands)
                        {
                            // Start building the command line with the command and sub-command names.
                            var line = $"{command.Name} {sub.Name}";

                            // Append each parameter with its explicit name.
                            if (sub.Parameters != null)
                            {
                                foreach (var param in sub.Parameters)
                                {
                                    // Use --paramName followed by a placeholder for its value.
                                    line += $" --{param.Name} {{{param.Name}}}";
                                }
                            }

                            commandLines.Add($"dxc {line}");
                        }
                    }
                }
            }

            return commandLines;
        }
    }
}
