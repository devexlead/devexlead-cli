using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;
using DevEx.Modules.SSL.Helpers;

namespace DevEx.Modules.SSL.Handlers
{
    public class SslCreateCertificateHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var sslConfigurationPath = CertificateHelper.GetSslConfigPath();
            var sslCertificatePassword = UserStorageManager.GetDecryptedValue("SslCertificatePassword");
            var sslCertificateDomain = UserStorageManager.GetDecryptedValue("SslCertificateDomain");
            var sslConfiguration = "ssl.cnf";

            //Generate the CA Private Key
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Wsl, $"openssl genrsa -out ca.key 4096", sslConfigurationPath);

            //Generate the Self-Signed CA Certificate
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Wsl, $"openssl req -x509 -new -nodes -key ca.key -sha256 -days 3650 -out ca.crt -subj '/C=AU/ST=Global/L=Global/O=DevExLead/OU=IT/CN=DevExLead Root CA'", sslConfigurationPath);

            //Generate the Wildcard Private Key
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Wsl, $"openssl genrsa -out wildcard.key 2048", sslConfigurationPath);

            //Create a CSR (Certificate Signing Request)
            CertificateHelper.GenerateSslConfiguration(sslConfigurationPath, sslConfiguration, sslCertificateDomain);
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Wsl, $"openssl req -new -key wildcard.key -out wildcard.csr -config {sslConfiguration}", sslConfigurationPath);

            //Sign the Wildcard Certificate with the CA Root Certificate
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Wsl, $"openssl x509 -req -in wildcard.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out wildcard.crt -days 825 -sha256 -extfile ssl.cnf -extensions req_ext", sslConfigurationPath);

            //Generate the PFX (required for IIS)
            TerminalHelper.Run(TerminalHelper.ConsoleMode.Wsl, $"openssl pkcs12 -export -out wildcard.pfx -inkey wildcard.key -in wildcard.crt -passout pass:{sslCertificatePassword}", sslConfigurationPath);
        }
    }
}
