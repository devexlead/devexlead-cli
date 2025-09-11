using System.Security.Cryptography.X509Certificates;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Model.Enums;
using DevExLead.Core.Storage;
using DevExLead.Modules.SSL.Helpers;

namespace DevExLead.Modules.SSL.Handlers
{
    internal class SslInstallCertificateHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var sslConfigPath = CertificateHelper.GetSslConfigPath();
            var sslCertificatePassword = UserStorageManager.GetDecryptedValue("SslCertificatePassword");

            //Delete existing certificates
            CertificateHelper.DeleteCertificate($"DevExLead Root CA", new X509Store(StoreName.Root, StoreLocation.LocalMachine));
            CertificateHelper.DeleteCertificate($"DevExLead Wildcard", new X509Store(StoreName.My, StoreLocation.LocalMachine));

            //Add to the stores
            TerminalHelper.Run(PromptModeEnum.Powershell, $"certutil -addstore root ca.crt", sslConfigPath);
            TerminalHelper.Run(PromptModeEnum.Powershell, $"certutil -addstore my wildcard.crt", sslConfigPath);
            TerminalHelper.Run(PromptModeEnum.Powershell, $"Import-PfxCertificate -FilePath 'wildcard.pfx' -Password (ConvertTo-SecureString -String '{sslCertificatePassword}' -AsPlainText -Force) -CertStoreLocation 'Cert:\\LocalMachine\\My'", sslConfigPath);
        }
    }
}
