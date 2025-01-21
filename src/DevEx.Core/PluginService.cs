using System.CommandLine;

namespace DevEx.Core
{
    public class PluginService
    {
        public static void LoadPlugins(RootCommand rootCommand)
        {
            var pluginAssemblies = Directory.GetFiles(AppContext.BaseDirectory, "DevEx.Plugins.*.dll");
            foreach (var assemblyPath in pluginAssemblies)
            {
                try
                {
                    var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

                    var pluginTypes = assembly.GetTypes()
                                              .Where(type => typeof(IPlugin)
                                              .IsAssignableFrom(type) && !type.IsInterface);

                    foreach (var type in pluginTypes)
                    {
                        if (Activator.CreateInstance(type) is IPlugin plugin)
                        {
                            #if !DEBUG
                            Console.WriteLine($"Loading plugin: {plugin.Name} - {plugin.Description}");
                            #endif
                            var command = plugin.GetCommand();
                            rootCommand.AddCommand(command);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load plugins from {assemblyPath}: {ex.Message}");
                }
            }
        }
    }
}
