using DevExLead.Core.Model.Command;
using DevExLead.Core.Storage;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace DevExLead.Cli
{
    public class CommandsHelper
    {
        public static void LoadCommands(RootCommand rootCommand, DxcCommands commandList)
        {
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
                                    IsRequired = param.IsRequired
                                };
                                optionsList.Add(option);
                                optionsMap[param.Name] = option;
                                subCommand.AddOption(option);
                            }
                        }

                        // Assign handler dynamically, passing all options as a dictionary
                        subCommand.SetHandler(async (context) =>
                        {
                            var handlerInstance = GetHandlerInstance(subCmd.Handler);
                            var optionsDict = new Dictionary<string, object>();

                            // Retrieve option values dynamically
                            foreach (var optionEntry in optionsMap)
                            {
                                string optionValue = context.ParseResult.GetValueForOption(optionEntry.Value);
                                if (!string.IsNullOrEmpty(optionValue))
                                {
                                    optionsDict[optionEntry.Key] = optionValue;
                                }
                            }

                            await handlerInstance.ExecuteAsync(optionsDict);
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
            foreach (var dllFile in Directory.GetFiles(assemblyDirectory, "DevExLead.Modules.*.dll"))
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

        public static void UpdateCommandsBookmark(DxcCommands dxcCommands)
        {
            //Clean PSReadLine File
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string psReadLineFile = Path.Combine(userFolder, "AppData\\Roaming\\Microsoft\\Windows\\PowerShell\\PSReadLine\\ConsoleHost_history.txt");
            File.WriteAllText(psReadLineFile, string.Empty);

            var userStorage = UserStorageManager.GetUserStorage();

            List<string> bookmarks = UpdateBookmarks(userStorage, dxcCommands);
            File.AppendAllLines(psReadLineFile, bookmarks);
        }

        private static List<string> UpdateBookmarks(UserStorage userStorage, DxcCommands dxcCommands)
        {
            //Insert CLI Standard Commands
            var commands = GenerateAllCommandCombinations(dxcCommands);

            //Insert Single User-Defined Commands
            foreach (var userDefinedCommand in userStorage.Commands)
            {
                commands.Add($"dxc command run --single \"{userDefinedCommand.Key}\"");
            }

            //Insert Multiple User-Defined Commands
            var groups = userStorage.Commands.Where(c => c.Group != null).Select(c => c.Group).Distinct().ToList();
            foreach (var userDefinedCommandGroup in groups)
            {
                commands.Add($"dxc command run --multiple \"{userDefinedCommandGroup}\"");
            }

            //Insert Repository Operations
            foreach (var repository in userStorage.Repositories)
            {
                commands.Add($"dxc git latest --key \"{repository.Key}\"");
                commands.Add($"dxc git latest --key \"{repository.Key}\" --branch \"{{Branch}}\"");
                commands.Add($"dxc azdo release --repository \"{repository.Key}\"");
            }

            commands.Add($"dxc --config");
            return commands;
        }

        /// <summary>
        /// Generates all possible command combinations with and without optional parameters
        /// Enhanced to handle DefaultValue with pipe-separated options
        /// </summary>
        /// <param name="dxcCommands">DxcCommands object</param>
        /// <returns>List of all possible command combinations as strings</returns>
        private static List<string> GenerateAllCommandCombinations(DxcCommands dxcCommands)
        {
            if (dxcCommands?.Commands == null)
                return new List<string>();

            var combinations = new List<string>();

            foreach (var command in dxcCommands.Commands)
            {
                foreach (var subCommand in command.SubCommands)
                {
                    // Handle null Parameters
                    var parameters = subCommand.Parameters ?? new DxcParameter[0];
                    
                    var requiredParams = parameters
                        .Where(p => p.IsRequired)
                        .ToList();

                    var optionalParams = parameters
                        .Where(p => !p.IsRequired)
                        .ToList();

                    // Check if any parameters have pipe-separated DefaultValue
                    var paramsWithPipeOptions = parameters
                        .Where(p => p.DefaultValue != null && !string.IsNullOrEmpty(p.DefaultValue.ToString()) && p.DefaultValue.ToString().Contains("|"))
                        .ToList();

                    // If there are no parameters at all, just add the basic command
                    if (!parameters.Any())
                    {
                        combinations.Add($"dxc {command.Name} {subCommand.Name}");
                        continue;
                    }

                    // If there are parameters with pipe-separated options, generate combinations for those
                    if (paramsWithPipeOptions.Any())
                    {
                        var pipeOptionCombinations = GeneratePipeOptionCombinations(paramsWithPipeOptions);
                        
                        foreach (var pipeOptions in pipeOptionCombinations)
                        {
                            // Generate all combinations of optional parameters (power set) for this pipe option combination
                            var optionalCombinations = GeneratePowerSet(optionalParams.Where(p => !pipeOptions.ContainsKey(p.Name)));

                            foreach (var optionalCombination in optionalCombinations)
                            {
                                var allParams = new List<string>();
                                
                                // Add required parameters
                                foreach (var requiredParam in requiredParams)
                                {
                                    if (pipeOptions.ContainsKey(requiredParam.Name))
                                    {
                                        allParams.Add($"--{requiredParam.Name} {pipeOptions[requiredParam.Name]}");
                                    }
                                    else
                                    {
                                        allParams.Add($"--{requiredParam.Name} <{requiredParam.Name}>");
                                    }
                                }

                                // Add pipe option parameters
                                foreach (var pipeOption in pipeOptions.Where(po => !requiredParams.Any(rp => rp.Name == po.Key)))
                                {
                                    allParams.Add($"--{pipeOption.Key} {pipeOption.Value}");
                                }

                                // Add optional parameters
                                foreach (var optionalParam in optionalCombination)
                                {
                                    allParams.Add($"--{optionalParam.Name} <{optionalParam.Name}>");
                                }

                                var paramString = allParams.Any()
                                    ? " " + string.Join(" ", allParams)
                                    : "";

                                combinations.Add($"dxc {command.Name} {subCommand.Name}{paramString}");
                            }
                        }
                    }
                    else
                    {
                        // Original logic for parameters without pipe-separated options
                        var optionalCombinations = GeneratePowerSet(optionalParams);

                        foreach (var optionalCombination in optionalCombinations)
                        {
                            var allParams = requiredParams.Concat(optionalCombination).ToList();

                            // Skip combinations that don't include required parameters when they exist
                            if (requiredParams.Any() && !allParams.Any())
                                continue;

                            var paramString = allParams.Any()
                                ? " " + string.Join(" ", allParams.Select(p => $"--{p.Name} <{p.Name}>"))
                                : "";

                            combinations.Add($"dxc {command.Name} {subCommand.Name}{paramString}");
                        }
                    }
                }
            }

            return combinations.OrderBy(c => c).ToList();
        }

        /// <summary>
        /// Generates all combinations of parameters that have pipe-separated default values
        /// </summary>
        /// <param name="parametersWithPipeOptions">Parameters that have DefaultValue with pipe-separated options</param>
        /// <returns>All possible combinations of parameter-value pairs</returns>
        private static IEnumerable<Dictionary<string, string>> GeneratePipeOptionCombinations(List<DxcParameter> parametersWithPipeOptions)
        {
            if (!parametersWithPipeOptions.Any())
            {
                yield return new Dictionary<string, string>();
                yield break;
            }

            var parameterOptions = parametersWithPipeOptions.ToDictionary(
                p => p.Name,
                p => p.DefaultValue.ToString().Split('|', StringSplitOptions.RemoveEmptyEntries).Select(opt => opt.Trim()).ToArray()
            );

            var combinations = GenerateCartesianProduct(parameterOptions);
            
            foreach (var combination in combinations)
            {
                yield return combination;
            }
        }

        /// <summary>
        /// Generates cartesian product of parameter options
        /// </summary>
        /// <param name="parameterOptions">Dictionary of parameter names and their possible values</param>
        /// <returns>All possible combinations</returns>
        private static IEnumerable<Dictionary<string, string>> GenerateCartesianProduct(Dictionary<string, string[]> parameterOptions)
        {
            if (!parameterOptions.Any())
            {
                yield return new Dictionary<string, string>();
                yield break;
            }

            var keys = parameterOptions.Keys.ToArray();
            var values = parameterOptions.Values.ToArray();
            var indices = new int[keys.Length];

            do
            {
                var combination = new Dictionary<string, string>();
                for (int i = 0; i < keys.Length; i++)
                {
                    combination[keys[i]] = values[i][indices[i]];
                }
                yield return combination;
            } while (IncrementIndices(indices, values));
        }

        /// <summary>
        /// Helper method to increment indices for cartesian product generation
        /// </summary>
        /// <param name="indices">Current indices</param>
        /// <param name="arrays">Arrays of values</param>
        /// <returns>True if increment was successful, false if we've reached the end</returns>
        private static bool IncrementIndices(int[] indices, string[][] arrays)
        {
            for (int i = indices.Length - 1; i >= 0; i--)
            {
                indices[i]++;
                if (indices[i] < arrays[i].Length)
                {
                    return true;
                }
                indices[i] = 0;
            }
            return false;
        }

        /// <summary>
        /// Generates the power set (all possible subsets) of a list
        /// </summary>
        /// <param name="items">List of items to generate power set for</param>
        /// <returns>All possible combinations including empty set</returns>
        private static IEnumerable<IEnumerable<T>> GeneratePowerSet<T>(IEnumerable<T> items)
        {
            var itemsList = items.ToList();
            var powerSetSize = 1 << itemsList.Count; // 2^n

            for (int i = 0; i < powerSetSize; i++)
            {
                var subset = new List<T>();
                for (int j = 0; j < itemsList.Count; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        subset.Add(itemsList[j]);
                    }
                }
                yield return subset;
            }
        }
    }
}