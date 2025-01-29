using DevEx.Core;

namespace DevEx.Modules.Vault.Windows.Handlers
{
    public class WindowsVaultUpdateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            Console.WriteLine("Executing Vault Update Command:");
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
