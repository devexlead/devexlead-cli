using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Command.Handlers
{
    internal class CommandUpdateCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var key = ParameterHelper.ReadStringParameter(options, "key");
            var body = ParameterHelper.ReadStringParameter(options, "body");
            var path = ParameterHelper.ReadStringParameter(options, "path");
            var group = ParameterHelper.ReadStringParameter(options, "group");
            var process = ParameterHelper.ReadStringParameter(options, "process");

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("--name --body are required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Commands.RemoveAll(c => c.Key == key);

            var command = new Core.Storage.Model.Command()
            {
                Key = key,
                Path = path,
                Body = body,
                Group = group,
                Process = process
            };

            userStorage.Commands.Add(command);
            UserStorageManager.SaveUserStorage(userStorage);
        }
    }
}
