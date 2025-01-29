namespace DevEx.Core.Handlers.Vault
{
    public class VaultCreateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            Console.WriteLine("Executing Vault Create Command:");
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
