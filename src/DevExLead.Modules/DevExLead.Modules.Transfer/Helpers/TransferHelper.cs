using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExLead.Modules.Transfer.Helpers
{
    internal class TransferHelper
    {
        internal static void SelectPath(out string folderPath)
        {
            folderPath = AnsiConsole.Ask<string>("Please enter the folder location:");
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                AnsiConsole.MarkupLine("[red]Invalid folder location.[/]");
                return;
            }
        }
    }
}
