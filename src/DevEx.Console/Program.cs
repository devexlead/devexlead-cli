using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DevEx.Core;
using DevEx.Core.Storage;

var rootCommand = new RootCommand();

PluginService.LoadPlugins(rootCommand);
UserStorageManager.Initialize();

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseSuggestDirective();

var parser = builder.Build();
return await parser.InvokeAsync(args);