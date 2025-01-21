using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DevEx.Core;

var rootCommand = new RootCommand();

PluginService.LoadPlugins(rootCommand);

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseSuggestDirective();

var parser = builder.Build();
return await parser.InvokeAsync(args);