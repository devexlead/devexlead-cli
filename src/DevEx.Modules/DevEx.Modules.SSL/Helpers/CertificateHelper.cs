using System.Security.Cryptography.X509Certificates;
using HandlebarsDotNet;

namespace DevEx.Modules.SSL.Helpers
{
    public class CertificateHelper
    {
        public static string GetSslConfigPath()
        {
            string sslConfigPath = Path.Combine(AppContext.BaseDirectory, "Configuration");
            return sslConfigPath;
        }

        public static void DeleteCertificate(string certificateName, X509Store store)
        {
            // Open the local machine's Trusted Root store (or you can use CurrentUser store)
            using (store)
            {
                try
                {
                    // Open the store in ReadWrite mode
                    store.Open(OpenFlags.ReadWrite);

                    // Find the certificate by name
                    var certificates = store.Certificates.Find(
                        X509FindType.FindBySubjectName, certificateName, false);

                    if (certificates.Count > 0)
                    {
                        foreach (var cert in certificates)
                        {
                            Console.WriteLine($"Deleting certificate: {cert.Subject}");
                            // Remove the certificate from the store
                            store.Remove(cert);
                        }

                        Console.WriteLine("Certificate(s) deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Certificate not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                finally
                {
                    // Close the store
                    store.Close();
                }
            }

        }

        public static void GenerateSslConfiguration(string sslConfigurationPath, string sslConfiguration, string domain)
        {
            var sslTemplate = "ssl.template";

            // Read the template file
            var templateSource = File.ReadAllText(Path.Combine(sslConfigurationPath, sslTemplate));

            // Compile the template
            var template = Handlebars.Compile(templateSource);

            // Prepare data for template rendering
            var data = new { domain };

            // Render the template with the domain data
            var result = template(data);

            // Write the rendered content to the output file
            File.WriteAllText(Path.Combine(sslConfigurationPath, sslConfiguration), result);
        }
    }
}
