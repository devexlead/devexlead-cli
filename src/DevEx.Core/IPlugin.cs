using System.CommandLine;

namespace DevEx.Core
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }
        Command GetCommand();
    }

}
