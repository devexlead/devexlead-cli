using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using DevEx.Core.Storage;
using DevEx.Modules.IntelliSense.Model;

namespace DevEx.Modules.IntelliSense.Helpers
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
                                    if (string.IsNullOrEmpty(param.DefaultValue))
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

            //Insert DevEx and Bookmarked CLI Commands into PSReadLine File
            var userStorage = UserStorageManager.GetUserStorage();
            var commands = IntelliSenseHelper.GetCommandLinesFromFile($"{AppContext.BaseDirectory}\\Commands.json");
            commands.AddRange(userStorage.Bookmarks);

            File.AppendAllLines(psReadLineFile, commands);
            //AnsiConsole.MarkupLine($"[Green]DevEx CLI IntelliSense is updated. Open a new PowerShell terminal.[/]");
            RestartTerminalInSameContext();
        }

        static void RestartTerminalInSameContext()
        {
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;

            // Detect the parent process (which terminal launched this app)
            string parentProcessName = GetParentProcessName();

            Console.WriteLine($"\nDetected parent process: {parentProcessName}");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (parentProcessName.Contains("powershell", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.FileName = "powershell";
                    startInfo.Arguments = $"-NoExit -Command \"& '{executablePath}'\"";
                }
                else
                {
                    startInfo.FileName = "cmd";
                    startInfo.Arguments = $"/k \"{executablePath}\"";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo.FileName = "bash";
                startInfo.Arguments = $"-c \"'{executablePath}'; exec bash\"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                startInfo.FileName = "open";
                startInfo.Arguments = $"-a Terminal '{executablePath}'";
                startInfo.UseShellExecute = true; // macOS requires this
            }
            else
            {
                Console.WriteLine("Unsupported OS");
                return;
            }

            try
            {
                Process.Start(startInfo);
                Console.WriteLine("Application restarted in the same terminal context.");
                Environment.Exit(0); // Close the current instance
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restarting: {ex.Message}");
            }
        }

        static string GetParentProcessName()
        {
            try
            {
                using var currentProcess = Process.GetCurrentProcess();
                int parentPid = 0;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: Use WMI to get parent process
                    var query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {currentProcess.Id}";
                    using var searcher = new System.Management.ManagementObjectSearcher(query);
                    var result = searcher.Get().Cast<System.Management.ManagementObject>().FirstOrDefault();
                    parentPid = result != null ? Convert.ToInt32(result["ParentProcessId"]) : 0;
                }
                else
                {
                    // Unix: Use ps command to get parent PID
                    var psi = new ProcessStartInfo
                    {
                        FileName = "ps",
                        Arguments = "-o ppid= -p " + currentProcess.Id,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    using var psProcess = Process.Start(psi);
                    var output = psProcess.StandardOutput.ReadToEnd().Trim();
                    parentPid = int.TryParse(output, out var pid) ? pid : 0;
                }

                if (parentPid > 0)
                {
                    var parentProcess = Process.GetProcessById(parentPid);
                    return parentProcess?.ProcessName ?? "Unknown";
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
