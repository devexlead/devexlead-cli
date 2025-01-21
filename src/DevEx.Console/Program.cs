using System.CommandLine;
using DevEx.Core;

var rootCommand = new RootCommand();

PluginService.LoadPlugins(rootCommand);

return await rootCommand.InvokeAsync(args);



