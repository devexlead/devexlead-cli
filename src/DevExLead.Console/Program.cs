using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DevEx.Console;
using DevEx.Core.Helpers;
using DevEx.Core.Storage;

var rootCommand = new RootCommand();

Helper.LoadCommands(rootCommand);

UserStorageManager.Initialize();

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseSuggestDirective();

var parser = builder.Build();
await parser.InvokeAsync(args);

//Refresh Intellisense Commands
IntelliSenseHelper.ResetPsReadLineFile();

return;

