using DevEx.Core;
using DevEx.Modules.IntelliSense.Helpers;

namespace DevEx.Modules.IntelliSense.Handlers
{
    public class CleanHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            IntelliSenseHelper.ResetPsReadLineFile();
        }
    }
}
