using DevExLead.Core;
using DevExLead.Core.Helpers;


namespace DevExLead.Modules.Tools.Handlers
{
    public class ToolsIntallHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            string installationConfigurationPath = Path.Combine(AppContext.BaseDirectory, "Installation");
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, "winget configure -f winget-config.yaml", installationConfigurationPath);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, ".\\post-install.ps1", installationConfigurationPath);
        }
    }
}
