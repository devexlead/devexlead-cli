using DevExLead.Core;
using DevExLead.Modules.Git.Helpers;

namespace DevExLead.Modules.Git.Handlers
{
    public class GitLatestHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var key = options["repository"];
            var branch = options["branch"];

            var repository = GitHelper.GetRepositories().FirstOrDefault(r => r.Key.Equals(key));
            if (repository != null)
            {
                GitHelper.GetLatest(repository, branch);
            }
            
        }
    }
}
