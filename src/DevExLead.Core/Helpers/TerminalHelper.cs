using System.Diagnostics;
using Spectre.Console;

namespace DevExLead.Core.Helpers
{
    public static class TerminalHelper
    {
        public static void Run(ConsoleMode consoleMode, string command, string? directory = null)
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
            var processStartInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments
            };

            if (!string.IsNullOrEmpty(directory))
            {
                processStartInfo.WorkingDirectory = directory;
            }

            return processStartInfo;
        }

        private static ProcessStartInfo BuildCommandArguments(ConsoleMode consoleMode, string command, string? directory)
        {
            var (filename, arguments) = consoleMode switch
            {
                ConsoleMode.Cmd => ("cmd.exe", $"/C {command}"),
                ConsoleMode.Powershell => ("powershell.exe", $"-noprofile -nologo -c {command}"),
                ConsoleMode.Wsl => ("ubuntu", $"run \"{command}\""),
                _ => throw new ArgumentOutOfRangeException(nameof(consoleMode), consoleMode, null)
            };

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
