﻿using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using DevEx.Core.Model.Command;

namespace DevEx.Core
{
    public class Commandservice
    {
        public static void LoadCommands(RootCommand rootCommand)
        {
            // Read and parse the JSON file
            string jsonFilePath = $"{AppContext.BaseDirectory}\\Commands.json"; // Ensure this file is placed in the project directory
            string jsonContent = File.ReadAllText(jsonFilePath);
            CommandDefinition commandDefinition = JsonSerializer.Deserialize<CommandDefinition>(jsonContent);

            // Build Commands dynamically
            foreach (var cmd in commandDefinition.Commands)
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
                        subCommand.SetHandler((InvocationContext context) =>
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

        static ICommandHandler InstantiateHandler(string handlerClassName)
        {
            Type handlerType = Type.GetType(handlerClassName);
            if (handlerType == null || !typeof(ICommandHandler).IsAssignableFrom(handlerType))
            {
                throw new InvalidOperationException($"Handler class '{handlerClassName}' not found or does not implement ICommandHandler.");
            }

            return (ICommandHandler)Activator.CreateInstance(handlerType);
        }

        static ICommandHandler GetHandlerInstance(string handlerName)
        {
            return InstantiateHandler(handlerName);
        }
    }
}