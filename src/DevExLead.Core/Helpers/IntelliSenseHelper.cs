using System.Text.Json;
using DevExLead.Core.Model.Command;
using DevExLead.Core.Storage;
using Spectre.Console;

namespace DevExLead.Core.Helpers
{
    public class IntelliSenseHelper
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
            var root = JsonSerializer.Deserialize<DxcCommands>(json, options);

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
                                    if (param.DefaultValue == null)
                                    {
                                        // Use --paramName followed by a placeholder for its value.
                                        line += $" --{param.Name} {{{param.Name}}}";
                                    }
                                    else
                                    {
                                        // Use --paramName followed by its default value.
                                        line += $" --{param.Name} {param.DefaultValue}";
                                    }
                                }
                            }

                            commandLines.Add($"dxc {line}");
                        }
                    }
                }
            }

            return commandLines;
        }

        public static void ResetPsReadLineFile()
        {
            //Clean PSReadLine File
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string psReadLineFile = Path.Combine(userFolder, "AppData\\Roaming\\Microsoft\\Windows\\PowerShell\\PSReadLine\\ConsoleHost_history.txt");
            File.WriteAllText(psReadLineFile, string.Empty);

            var userStorage = UserStorageManager.GetUserStorage();

            //Insert CLI Standard Commands
            var commands = GetCommandLinesFromFile($"{AppContext.BaseDirectory}\\Commands.json");

            //Insert Single User-Defined Commands
            foreach (var userDefinedCommand in userStorage.Commands)
            {
                commands.Add($"dxc command run --single \"{userDefinedCommand.Key}\"");
            }

            //Insert Multiple User-Defined Commands
            var groups = userStorage.Commands.Where(c => c.Group != null).Select(c => c.Group).Distinct().ToList();
            foreach (var userDefinedCommandGroup in groups)
            {
                commands.Add($"dxc command run --multiple \"{userDefinedCommandGroup}\"");
            }

            //Insert Repository Operations
            foreach (var repository in userStorage.Repositories)
            {
                commands.Add($"dxc git latest --key \"{repository.Key}\"");
                commands.Add($"dxc git latest --key \"{repository.Key}\" --branch \"{{Branch}}\"");
            }

            commands.Add($"dxc --config");

            File.AppendAllLines(psReadLineFile, commands);
        }
    }
}
