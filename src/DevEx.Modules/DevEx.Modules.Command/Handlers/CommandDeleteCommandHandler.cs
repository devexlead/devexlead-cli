﻿using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.Command.Handlers
{
    internal class CommandDeleteCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var name = options["name"];

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("--name is required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();

            //remove existing application with the same name
            userStorage.Commands.RemoveAll(c => c.Name == name);
            UserStorageManager.SaveUserStorage(userStorage);
        }
    }
}
