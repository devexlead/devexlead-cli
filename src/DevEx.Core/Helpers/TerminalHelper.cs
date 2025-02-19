using System.Diagnostics;
using Spectre.Console;

namespace DevEx.Core.Helpers
{
    public static class TerminalHelper
    {


        public static void Run(ConsoleMode consoleMode, string command, string directory = null)
        {
            try
            {
                DisplayLogInConsole(command, directory);
                var processStartInfo = BuildCommandArguments(consoleMode, command, directory);
                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
        }

        private static ProcessStartInfo InitializeProcessStartInfo(string? directory, string filename, string arguments)
        {
            var processStartInfo = new ProcessStartInfo();
            if (!string.IsNullOrEmpty(directory))
            {
                processStartInfo.WorkingDirectory = directory;
            }
            processStartInfo.FileName = filename;
            processStartInfo.Arguments = arguments;
            return processStartInfo;
        }

        private static ProcessStartInfo BuildCommandArguments(ConsoleMode consoleMode, string command, string directory)
        {
            var filename = string.Empty;
            var arguments = string.Empty;
            switch (consoleMode)
            {
                case ConsoleMode.Cmd:
                    filename = "cmd.exe";
                    arguments = $"/C {command}";
                    break;
                case ConsoleMode.Powershell:
                    filename = "powershell.exe";
                    arguments = $" -noprofile -nologo -c {command}";
                    break;
                case ConsoleMode.Wsl:
                    filename = "ubuntu";
                    arguments = $"run \"{command}\"";
                    break;
            }

            return InitializeProcessStartInfo(directory, filename, arguments);
        }

        private static void DisplayLogInConsole(string command, string? directory)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Command:   {command}[/]");
            if (!string.IsNullOrEmpty(directory))
            {
                AnsiConsole.MarkupLine($"[yellow]Directory: {directory}[/]");
            }
        }

        public enum ConsoleMode
        {
            Cmd,
            Powershell,
            Wsl
        }
    }
}