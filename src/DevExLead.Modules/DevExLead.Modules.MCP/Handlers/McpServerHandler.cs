using DevExLead.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

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
                .WithToolsFromAssembly(typeof(McpServerTool).Assembly);

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
