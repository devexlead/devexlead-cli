using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DevExLead.Cli;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

var rootCommand = new RootCommand();

Helper.LoadCommands(rootCommand);

UserStorageManager.Initialize();

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseSuggestDirective();

var parser = builder.Build();

///*TEMP************************************************************************************/
//var reEncryptedValues = new Dictionary<string, string>();
//var userStorage = UserStorageManager.GetUserStorage();
//foreach (var item in userStorage.Vault)
//{
//    var decrypted = SecurityHelper.DecryptKey(item.Value);
//    Console.WriteLine($"{item.Key} : {decrypted}");
//    reEncryptedValues.Add(item.Key, SecurityHelper.EncryptVaultEntry(decrypted));
//}

//userStorage.Vault.Clear();
//UserStorageManager.SaveUserStorage(userStorage);

//userStorage = UserStorageManager.GetUserStorage();
//foreach (var item in reEncryptedValues)
//{
//    userStorage.Vault.Add(item.Key, item.Value);
//}

//UserStorageManager.SaveUserStorage(userStorage);

///*TEMP************************************************************************************/

await parser.InvokeAsync(args);

//Refresh Intellisense Commands
IntelliSenseHelper.ResetPsReadLineFile();



return;

