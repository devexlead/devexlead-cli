using DevExLead.Core;
using WaffleGenerator;

namespace DevExLead.Modules.Tools.Handlers
{
    public class WaffleGeneratorHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var paragraphs = options["paragraphs"];
            var includeHeading = options["includeHeading"];

            if (string.IsNullOrWhiteSpace(paragraphs) || string.IsNullOrWhiteSpace(includeHeading))
            {
                Console.WriteLine("Both --paragraphs and --includeHeading are required for WaffleGenerator.");
                return;
            }

            // Attempt to parse paragraphs as an integer.
            if (!int.TryParse(paragraphs, out int paragraphCount))
            {
                Console.WriteLine("Error: The value provided for --paragraphs is not a valid integer.");
                return;
            }

            // Attempt to parse includeHeading as a boolean.
            if (!bool.TryParse(includeHeading, out bool includeHeaderBool))
            {
                Console.WriteLine("Error: The value provided for --includeHeading is not a valid boolean. Use 'true' or 'false'.");
                return;
            }

            var text = WaffleEngine.Text(paragraphs: paragraphCount, includeHeading: includeHeaderBool);
            Console.WriteLine(text);
        }
    }
}
