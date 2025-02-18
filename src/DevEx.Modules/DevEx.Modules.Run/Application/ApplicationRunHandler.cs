using DevEx.Core;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;

namespace DevEx.Modules.Run.Application
{
    public class ApplicationRunHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var applicationName = options["name"];

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                Console.WriteLine("--name is required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();
            var application = userStorage.Applications.FirstOrDefault(a => a.Name == applicationName);

            if (application == null)
            {
                Console.WriteLine("Application not found.");
                return;
            }

            TerminalHelper.Run(TerminalHelper.ConsoleMode.Powershell, application.Command, application.Path);
        }
    }
}
