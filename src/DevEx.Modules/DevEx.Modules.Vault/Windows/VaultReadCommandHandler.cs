using DevEx.Core;

namespace DevEx.Modules.Vault.Windows
{
    public class VaultReadCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            Console.WriteLine("Executing Vault Read Command:");
            PrintOptions(options);
        }

        private void PrintOptions(Dictionary<string, string> options)
        {
            foreach (var option in options)
            {
                Console.WriteLine($"{option.Key}: {option.Value}");
            }
        }
    }
}
