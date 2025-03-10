using DevEx.Core;
using DevEx.Core.Helpers;

namespace DevEx.Modules.EnvironmentVariables.Handlers
{
    public class EnvironmentVariableListHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            EnvironmentVariableHelper.ConfigureMachineEnvironmentVariables();
        }
    }
}
