using DevEx.Core;
using DevEx.Modules.Git.Helpers;

namespace DevEx.Modules.Git.Handlers
{
    public class GitLatestHandler : ICommandHandler
    {
        public void Execute(Dictionary<string, string> options)
        {
            var repositories = GitHelper.GetRepositories();
            foreach (var repository in repositories)
            {
                GitHelper.GetLatest(repository);
            }
        }
    }
}
