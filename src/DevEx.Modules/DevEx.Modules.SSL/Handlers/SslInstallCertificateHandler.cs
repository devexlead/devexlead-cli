using System.Security.Cryptography.X509Certificates;
using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Modules.SSL.Helpers;

namespace DevEx.Modules.SSL.Handlers
{
    internal class SslInstallCertificateHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var sslConfigPath = CertificateHelper.GetSslConfigPath();
            var password = "d3v3xl0c4l";
            var domain = "devexlocal";

            //Delete existing certificates
            CertificateHelper.DeleteCertificate($"DevExLead Root CA", new X509Store(StoreName.Root, StoreLocation.LocalMachine));
            CertificateHelper.DeleteCertificate($"DevExLead Wildcard", new X509Store(StoreName.My, StoreLocation.LocalMachine));

            //Add to the stores
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"certutil -addstore root ca.crt", sslConfigPath);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"certutil -addstore my wildcard.crt", sslConfigPath);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, $"Import-PfxCertificate -FilePath 'wildcard.pfx' -Password (ConvertTo-SecureString -String '{password}' -AsPlainText -Force) -CertStoreLocation 'Cert:\\LocalMachine\\My'", sslConfigPath);
        }
    }
}
