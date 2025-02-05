using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DevEx.Console;
using DevEx.Core.Storage;
using DevEx.Modules.IntelliSense.Helpers;

var rootCommand = new RootCommand();

Helper.LoadCommands(rootCommand);

UserStorageManager.Initialize();

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseSuggestDirective();


var userStorage = UserStorageManager.GetUserStorage();

//Check if IntelliSense must be updated
var currentVersion = Helper.GetCurrentVersion();
if (!currentVersion.Equals(userStorage.Version))
{
    IntelliSenseHelper.ResetPsReadLineFile();
    userStorage.Version = currentVersion;
    UserStorageManager.SaveUserStorage(userStorage);
}

var parser = builder.Build();
return await parser.InvokeAsync(args);

