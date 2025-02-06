using System.Diagnostics;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace DevEx.Core.Helpers
{
    public static class TerminalHelper
    {
        //public static string Run(ConsoleMode consoleMode, string command, string? directory = null, bool waitForExit = true, bool redirectStandardOutput = true, string regexPattern = null, bool isPrintable = true)
        //{
        //    if (isPrintable)
        //    {
        //        AnsiConsole.WriteLine();
        //        AnsiConsole.MarkupLine($"[yellow]Command:   {command}[/]");
        //        AnsiConsole.MarkupLine($"[yellow]Directory: {directory}[/]");
        //    }

        //    var filename = string.Empty;
        //    var arguments = string.Empty;

        //    switch (consoleMode)
        //    {
        //        case ConsoleMode.Cmd:
        //            filename = "cmd.exe";
        //            arguments = $"/C {command}";
        //            break;
        //        case ConsoleMode.Powershell:
        //            filename = "powershell.exe";
        //            arguments = $" -noprofile -nologo -c {command}";
        //            break;
        //        case ConsoleMode.Wsl:
        //            filename = "wsl";
        //            arguments = command;
        //            break;
        //    }

        //    try
        //    {
        //        var p = new Process();
        //        if (!string.IsNullOrEmpty(directory))
        //        {
        //            p.StartInfo.WorkingDirectory = directory;
        //        }
        //        p.StartInfo.UseShellExecute = false;
        //        //p.StartInfo.RedirectStandardInput = true;
        //        p.StartInfo.RedirectStandardOutput = redirectStandardOutput;
        //        p.StartInfo.FileName = filename;
        //        p.StartInfo.Arguments = arguments;
        //        p.Start();
        //        //p.StandardInput.WriteLine("Y");
        //        if (waitForExit)
        //        {
        //            var output = p.StandardOutput.ReadToEnd();
        //            p.WaitForExit();

        //            if (!string.IsNullOrEmpty(regexPattern))
        //            {
        //                Regex regex = new Regex(regexPattern);
        //                Match match = regex.Match(output);
        //                if (match.Success)
        //                {
        //                    output = match.Value;
        //                }
        //            }

        //            if (isPrintable)
        //            {
        //                AnsiConsole.MarkupLine($"[yellow]Output: {output.Replace("[", "[[").Replace("]", "]]")}[/]");
        //            }

        //            return output;
        //        }

        //        return string.Empty;
        //    }
        //    catch (Exception ex)
        //    {
        //        AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
        //        return string.Empty;
        //    }
        //}

        public static void Run(ConsoleMode consoleMode, string command, string directory = null)
        {
            try
            {
                string filename, arguments;
                DisplayLogInConsole(command, directory);
                BuildCommand(consoleMode, command, out filename, out arguments);
                Process p = StartProcess(directory, filename, arguments, true);
                while (!p.HasExited)
                {
                    var output = p.StandardOutput.ReadLine();
                    AnsiConsole.MarkupLine($"[yellow]{output?.Replace("[", "[[").Replace("]", "]]")}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
        }

        private static Process StartProcess(string? directory, string filename, string arguments, bool redirectStandard)
        {
            var p = new Process();
            if (!string.IsNullOrEmpty(directory))
            {
                p.StartInfo.WorkingDirectory = directory;
            }
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = redirectStandard;
            p.StartInfo.RedirectStandardError = redirectStandard;
            p.StartInfo.FileName = filename;
            p.StartInfo.Arguments = arguments;
            p.Start();
            return p;
        }

        private static void BuildCommand(ConsoleMode consoleMode, string command, out string filename, out string arguments)
        {
            filename = string.Empty;
            arguments = string.Empty;
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