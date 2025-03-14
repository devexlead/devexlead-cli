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

//Refresh Intellisense Commands
IntelliSenseHelper.ResetPsReadLineFile();

var parser = builder.Build();
return await parser.InvokeAsync(args);

