using DevEx.Core;
using DevEx.Modules.Git.Helpers;

namespace DevEx.Modules.Git.Handlers
{
    public class GitCloneHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var repositories = GitHelper.GetRepositories();
            foreach (var repository in repositories)
            {
                GitHelper.Clone(repository);
            }
        }
    }
}
