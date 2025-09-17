using System.ComponentModel;
using ModelContextProtocol.Server;

namespace DevExLead.Modules.MCP.Helpers;

[McpServerToolType]
public static class McpServerHelper
{
    [McpServerTool, Description("Echo the message back.")]
    public static string Echo(string message) => $"Echo: {message}";

    [McpServerTool, Description("Reverse the message.")]
    public static string ReverseEcho(string message)
    {
        var arr = message.Reverse().ToArray();
        return new string(arr);
    }
}
