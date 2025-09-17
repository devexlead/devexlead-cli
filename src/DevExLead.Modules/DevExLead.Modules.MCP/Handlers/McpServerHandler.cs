using DevExLead.Core;
using DevExLead.Modules.MCP.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevExLead.Modules.MCP.Handlers
{
    public class McpServerHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly(typeof(McpServerHelper).Assembly);

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
