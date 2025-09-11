using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Model.Enums;
using DevExLead.Core.Storage;
using DevExLead.Modules.SSL.Helpers;

namespace DevExLead.Modules.SSL.Handlers
{
    public class SslCreateCertificateHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var sslConfigurationPath = CertificateHelper.GetSslConfigPath();
            var sslCertificatePassword = UserStorageManager.GetDecryptedValue("SslCertificatePassword");
            var sslCertificateDomain = UserStorageManager.GetDecryptedValue("SslCertificateDomain");
            var sslConfiguration = "ssl.cnf";

            //Generate the CA Private Key
            TerminalHelper.Run(PromptModeEnum.Wsl, $"openssl genrsa -out ca.key 4096", sslConfigurationPath);

            //Generate the Self-Signed CA Certificate
            TerminalHelper.Run(PromptModeEnum.Wsl, $"openssl req -x509 -new -nodes -key ca.key -sha256 -days 3650 -out ca.crt -subj '/C=AU/ST=Global/L=Global/O=DevExLead/OU=IT/CN=DevExLead Root CA'", sslConfigurationPath);

            //Generate the Wildcard Private Key
            TerminalHelper.Run(PromptModeEnum.Wsl, $"openssl genrsa -out wildcard.key 2048", sslConfigurationPath);

            //Create a CSR (Certificate Signing Request)
            CertificateHelper.GenerateSslConfiguration(sslConfigurationPath, sslConfiguration, sslCertificateDomain);
            TerminalHelper.Run(PromptModeEnum.Wsl, $"openssl req -new -key wildcard.key -out wildcard.csr -config {sslConfiguration}", sslConfigurationPath);

            //Sign the Wildcard Certificate with the CA Root Certificate
            TerminalHelper.Run(PromptModeEnum.Wsl, $"openssl x509 -req -in wildcard.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out wildcard.crt -days 825 -sha256 -extfile ssl.cnf -extensions req_ext", sslConfigurationPath);

            //Generate the PFX (required for IIS)
            TerminalHelper.Run(PromptModeEnum.Wsl, $"openssl pkcs12 -export -out wildcard.pfx -inkey wildcard.key -in wildcard.crt -passout pass:{sslCertificatePassword}", sslConfigurationPath);
        }
    }
}
