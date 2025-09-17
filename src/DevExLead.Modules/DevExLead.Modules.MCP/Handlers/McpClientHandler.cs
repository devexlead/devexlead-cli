using DevExLead.Core;
using ModelContextProtocol.Client;
using Spectre.Console;
using System.Text.Json;

namespace DevExLead.Modules.MCP.Handlers
{
    public class McpClientHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "LocalServer",
                Command = "dxc",
                Arguments = new[] { "mcp", "server" }
            });

            try
            {
                AnsiConsole.MarkupLine("[blue]Connecting to MCP server...[/]");
                var client = await McpClientFactory.CreateAsync(transport);
                AnsiConsole.MarkupLine("[green]✓ Connected successfully![/]");

                // Display server information
                AnsiConsole.MarkupLine($"[cyan]Server: {client.ServerInfo?.Name ?? "Unknown"} v{client.ServerInfo?.Version ?? "Unknown"}[/]");
                
                // Check if the server supports tools
                if (client.ServerCapabilities?.Tools == null)
                {
                    AnsiConsole.MarkupLine("[yellow]Server does not support tools.[/]", transport);
                    return;
                }

                // Get available tools
                IList<McpClientTool> tools;
                try
                {
                    tools = await client.ListToolsAsync();
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to list tools: {ex.Message}[/]");
                    AnsiConsole.MarkupLine("[yellow]The server may not support the tools/list method.[/]", transport);
                    return;
                }

                if (!tools.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No tools available on the server.[/]", transport);
                    return;
                }

                // Display available tools in a table
                var table = new Table();
                table.AddColumn("[blue]Tool Name[/]");
                table.AddColumn("[blue]Description[/]");

                foreach (var tool in tools)
                {
                    table.AddRow(tool.Name, tool.Description ?? "No description");
                }

                AnsiConsole.Write(table);

                // Interactive loop for tool selection and execution
                while (true)
                {
                    // Tool selection
                    var toolChoices = tools.Select(t => t.Name).ToList();
                    toolChoices.Add("[red]Exit[/]");

                    var selectedTool = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("\n[green]Select a tool to execute:[/]")
                            .AddChoices(toolChoices)
                            .UseConverter(choice => choice == "[red]Exit[/]" ? "Exit" : choice)
                    );

                    if (selectedTool == "[red]Exit[/]")
                    {
                        AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
                        break;
                    }

                    // Get the selected tool details
                    var tool = tools.First(t => t.Name == selectedTool);

                    // Collect parameters for the tool
                    var parameters = new Dictionary<string, object?>();

                    // Simple parameter collection - you can enhance this based on tool schema
                    if (tool.JsonSchema.ValueKind != JsonValueKind.Undefined && 
                        tool.JsonSchema.TryGetProperty("properties", out var properties))
                    {
                        foreach (var property in properties.EnumerateObject())
                        {
                            var paramName = property.Name;
                            var paramValue = AnsiConsole.Ask<string>($"[cyan]Enter value for '{paramName}':[/]");
                            parameters[paramName] = paramValue;
                        }
                    }
                    else
                    {
                        // Fallback: ask for a generic message parameter
                        var message = AnsiConsole.Ask<string>("[cyan]Enter message (or leave empty):[/]", "");
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            parameters["message"] = message;
                        }
                    }

                    try
                    {
                        await AnsiConsole.Status()
                            .StartAsync($"[yellow]Executing {selectedTool}...[/]", async ctx =>
                            {
                                ctx.Spinner(Spinner.Known.Star);
                                ctx.SpinnerStyle(Style.Parse("green"));

                                // Execute the tool
                                var result = await tool.CallAsync(parameters);

                                // Display results
                                AnsiConsole.WriteLine();
                                var resultPanel = new Panel(FormatToolResult(result))
                                    .Header($"[green]Result from {selectedTool}[/]")
                                    .Border(BoxBorder.Rounded)
                                    .BorderColor(Color.Green);

                                AnsiConsole.Write(resultPanel);
                            });
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error executing tool: {ex.Message}[/]");
                    }

                    AnsiConsole.WriteLine();

                    // Ask if user wants to continue
                    var continueChoice = AnsiConsole.Confirm("[yellow]Execute another tool?[/]");
                    if (!continueChoice)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to connect to MCP server: {ex.Message}[/]");
            }
        }

        private static string FormatToolResult(ModelContextProtocol.Protocol.CallToolResult result)
        {
            if (result.IsError == true)
            {
                return $"[red]Error: {string.Join("\n", result.Content?.Select(c => c.ToString()) ?? [])}[/]";
            }

            if (result.Content?.Any() == true)
            {
                var content = result.Content
                    .Where(c => c.Type == "text")
                    .Select(c => c.ToString())
                    .FirstOrDefault();

                return content ?? "No text content returned";
            }

            if (result.StructuredContent != null)
            {
                return result.StructuredContent.ToString();
            }

            return "No content returned";
        }
    }
}