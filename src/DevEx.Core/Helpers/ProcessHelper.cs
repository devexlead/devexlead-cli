﻿using Spectre.Console;
using System.Diagnostics;
using System.Security.Principal;

namespace DevEx.Core.Helpers
{
    public static class ProcessHelper
    {
        public static void CleanUpProcesses(List<string> processes)
        {
            foreach (var process in processes)
            {
                AnsiConsole.MarkupLine($"[yellow]Terminating {process} process...[/]");
                Process[] workers = Process.GetProcessesByName(process);
                foreach (var worker in workers)
                {
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }
                AnsiConsole.MarkupLine($"[red]Process {process} has been terminated.[/]");
            }
        }

        public static bool IsRunningAsAdmin()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(currentUser);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isAdmin;
        }

        public static int? FindProcessId(string processName)
        {
            var pid = Process.GetProcessesByName(processName).FirstOrDefault().Id;
            AnsiConsole.MarkupLine($"[yellow]Process ID for {processName}.exe: {pid}[/]");
            return pid;
        }

    }
}
