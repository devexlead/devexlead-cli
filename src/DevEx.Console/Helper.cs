using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Text.Json;
using DevEx.Core.Model.Command;

namespace DevEx.Console
{
    public class Helper
    {
        public static void LoadCommands(RootCommand rootCommand)
        {
            // Read and parse the JSON file
            string jsonFilePath = $"{AppContext.BaseDirectory}\\Commands.json"; // Ensure this file is placed in the project directory
            string jsonContent = File.ReadAllText(jsonFilePath);
            DxcCommands commandList = JsonSerializer.Deserialize<DxcCommands>(jsonContent);

            // Build Commands dynamically
            foreach (var cmd in commandList.Commands)
            {
                var mainCommand = new Command(cmd.Name, cmd.Description);

                if (cmd.SubCommands != null)
                {
                    foreach (var subCmd in cmd.SubCommands)
                    {
                        var subCommand = new Command(subCmd.Name, subCmd.Description);

                        // Store options dynamically
                        var optionsList = new List<Option<string>>();
                        var optionsMap = new Dictionary<string, Option<string>>();

                        if (subCmd.Parameters != null)
                        {
                            foreach (var param in subCmd.Parameters)
                            {
                                var option = new Option<string>($"--{param.Name}", param.Description)
                                {
                                    IsRequired = false // Adjust based on needs
                                };
                                optionsList.Add(option);
                                optionsMap[param.Name] = option;
                                subCommand.AddOption(option);
                            }
                        }

                        // Assign handler dynamically, passing all options as a dictionary
                        subCommand.SetHandler((context) =>
                        {
                            var handlerInstance = GetHandlerInstance(subCmd.Handler);
                            var optionsDict = new Dictionary<string, string>();

                            // Retrieve option values dynamically
                            foreach (var optionEntry in optionsMap)
                            {
                                string optionValue = context.ParseResult.GetValueForOption(optionEntry.Value);
                                if (!string.IsNullOrEmpty(optionValue))
                                {
                                    optionsDict[optionEntry.Key] = optionValue;
                                }
                            }

                            handlerInstance.Execute(optionsDict);
                        });

                        mainCommand.AddCommand(subCommand);
                    }
                }

                rootCommand.AddCommand(mainCommand);
            }
        }

        static Core.ICommandHandler GetHandlerInstance(string handlerName)
        {
            Type handlerType = FindTypeInReferencedAssemblies(handlerName);
            if (handlerType == null || !typeof(Core.ICommandHandler).IsAssignableFrom(handlerType))
            {
                throw new InvalidOperationException($"Handler class '{handlerName}' not found or does not implement ICommandHandler.");
            }

            return (Core.ICommandHandler)Activator.CreateInstance(handlerType);
        }

        static Type FindTypeInReferencedAssemblies(string className)
        {
            string assemblyDirectory = AppContext.BaseDirectory;
            foreach (var dllFile in Directory.GetFiles(assemblyDirectory, "DevEx.Modules.*.dll"))
            {
                Assembly externalAssembly = Assembly.LoadFrom(dllFile);
                var type = externalAssembly.GetType(className, false, true);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static string GetCurrentVersion()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
        }


    }
}