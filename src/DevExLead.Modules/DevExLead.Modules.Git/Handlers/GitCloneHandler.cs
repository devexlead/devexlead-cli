using DevExLead.Core;
using DevExLead.Modules.Git.Helpers;

namespace DevExLead.Modules.Git.Handlers
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
