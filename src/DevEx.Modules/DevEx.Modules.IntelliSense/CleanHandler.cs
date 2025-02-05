using DevEx.Core;
using DevEx.Modules.IntelliSense.Helpers;
using Spectre.Console;

namespace DevEx.Modules.IntelliSense
{
    public class CleanHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            IntelliSenseHelper.ResetPsReadLineFile();
        }
    }
}
