//using System.CommandLine;
//using DevEx.Core;
//using DevEx.Core.Storage;

//namespace DevEx.Plugins.Vault
//{
//    public class VaultPlugin : IPlugin
//    {
//        public string Name => "vault";
//        public string Description => "Manage items in the vault";

//        public Command GetCommand()
//        {
//            var vaultCommand = new Command(Name, Description);

//            // Create subCommands for actions
//            Command createCommand = BuildCreateCommand();
//            Command deleteCommand = BuildDeleteCommand();
//            Command modifyCommand = BuildModifyCommand();
//            Command readCommand = BuildReadCommand();

//            // Add subCommands to the main vault command
//            vaultCommand.AddCommand(createCommand);
//            vaultCommand.AddCommand(deleteCommand);
//            vaultCommand.AddCommand(modifyCommand);
//            vaultCommand.AddCommand(readCommand);

//            return vaultCommand;
//        }

//        private static Command BuildReadCommand()
//        {
//            var readCommand = new Command("read", "Fetch an item from the vault")
//                                    {
//                                        new Option<string>("--key", "The key of the item to fetch")
//                                    };
//            readCommand.SetHandler((string key) =>
//            {
//                if (string.IsNullOrWhiteSpace(key))
//                {
//                    Console.WriteLine("--key is required for read.");
//                    return;
//                }

//                var userStorage = UserStorageManager.GetUserStorage();
//                var value = userStorage.Vault.FirstOrDefault(v => v.Key.Equals(key)).Value.Decrypt();
//                Console.WriteLine($"Fetched item with Key={key} and Value={value}");
//            },
//            readCommand.Options[0] as Option<string>);
//            return readCommand;
//        }

//        private static Command BuildModifyCommand()
//        {
//            var modifyCommand = new Command("modify", "Modify an item in the vault")
//                                        {
//                                            new Option<string>("--key", "The key of the item to modify"),
//                                            new Option<string>("--value", "The new value of the item")
//                                        };
//            modifyCommand.SetHandler((string key, string value) =>
//            {
//                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
//                {
//                    Console.WriteLine("Both --key and --value are required for modify.");
//                    return;
//                }

//                var userStorage = UserStorageManager.GetUserStorage();
//                userStorage.Vault[key] = value.Encrypt();
//                UserStorageManager.SaveUserStorage(userStorage);

//                Console.WriteLine($"Modified item: Key={key}, New Value={value}");
//            },
//            modifyCommand.Options[0] as Option<string>,
//            modifyCommand.Options[1] as Option<string>);
//            return modifyCommand;
//        }

//        private static Command BuildDeleteCommand()
//        {
//            var deleteCommand = new Command("delete", "Remove an item from the vault")
//                                        {
//                                            new Option<string>("--key", "The key of the item to remove")
//                                        };
//            deleteCommand.SetHandler((string key) =>
//            {
//                if (string.IsNullOrWhiteSpace(key))
//                {
//                    Console.WriteLine("--key is required for delete.");
//                    return;
//                }

//                var userStorage = UserStorageManager.GetUserStorage();
//                userStorage.Vault.Remove(key);
//                UserStorageManager.SaveUserStorage(userStorage);

//                Console.WriteLine($"Deleted item with Key={key}");
//            },
//            deleteCommand.Options[0] as Option<string>);
//            return deleteCommand;
//        }

//        private static Command BuildCreateCommand()
//        {
//            var createCommand = new Command("create", "Add an item to the vault")
//                                        {
//                                            new Option<string>("--key", "The key of the item to add"),
//                                            new Option<string>("--value", "The value of the item to add")
//                                        };
//            createCommand.SetHandler((string key, string value) =>
//            {
//                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
//                {
//                    Console.WriteLine("Both --key and --value are required for create.");
//                    return;
//                }

//                var userStorage = UserStorageManager.GetUserStorage();
//                userStorage.Vault.Add(key, value.Encrypt());
//                UserStorageManager.SaveUserStorage(userStorage);

//                Console.WriteLine($"Created item: Key={key}, Value={value}");
//            },
//            createCommand.Options[0] as Option<string>,
//            createCommand.Options[1] as Option<string>);
//            return createCommand;
//        }
//    }

//}
