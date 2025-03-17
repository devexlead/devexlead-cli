using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Model.Enums;


namespace DevExLead.Modules.Setup.Handlers
{
    public class SeptupInstallHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            string installationConfigurationPath = Path.Combine(AppContext.BaseDirectory, "Installation");
            TerminalHelper.Run(PromptModeEnum.Powershell, "winget configure -f winget-config.yaml", installationConfigurationPath);
            TerminalHelper.Run(PromptModeEnum.Powershell, ".\\post-install.ps1", installationConfigurationPath);
        }
    }
}
