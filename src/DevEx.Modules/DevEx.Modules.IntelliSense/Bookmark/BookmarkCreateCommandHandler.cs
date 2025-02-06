using DevEx.Core;
using DevEx.Core.Storage;

namespace DevEx.Modules.IntelliSense.Bookmark
{
    internal class BookmarkCreateCommandHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var command = options["command"];

            if (string.IsNullOrWhiteSpace(command))
            {
                Console.WriteLine("--command is required.");
                return;
            }

            var userStorage = UserStorageManager.GetUserStorage();
            userStorage.Bookmarks.Add(command);
            UserStorageManager.SaveUserStorage(userStorage);

            Console.WriteLine($"Bookmark has been created.");
        }
    }
}
