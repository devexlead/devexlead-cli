using DevExLead.Core.Model.Enums;
using Spectre.Console;
using System.Diagnostics;

namespace DevExLead.Core.Helpers
{
    public static class TerminalHelper
    {
        public static void Run(PromptModeEnum promptModeEnum, string command, string? directory = null, bool isMultipleExecution = false)
        {
            try
            {
                DisplayLogInConsole(command, directory);

                //Configure Prompt
                var (filename, arguments) = promptModeEnum switch
                {
                    PromptModeEnum.Cmd => ("cmd.exe", $"/C {command}"),
                    PromptModeEnum.Powershell => ("powershell.exe", $"-noprofile -nologo -c {command}"),
                    PromptModeEnum.Wsl => ("ubuntu", $"run \"{command}\"")
                };

                //Configure ProcessStartInfo
                var processStartInfo = ConfigureProcessStartInfo(directory, filename, arguments, isMultipleExecution);

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

        private static ProcessStartInfo ConfigureProcessStartInfo(string? directory, string filename, string arguments, bool isMultipleExecution)
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

            if (isMultipleExecution)
            {
                processStartInfo.UseShellExecute = true;
                processStartInfo.CreateNoWindow = false;
            }

            return processStartInfo;
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
    }
}
