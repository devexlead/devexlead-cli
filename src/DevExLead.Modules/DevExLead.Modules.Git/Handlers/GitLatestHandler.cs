using DevExLead.Core;
using DevExLead.Modules.Git.Helpers;

namespace DevExLead.Modules.Git.Handlers
{
    public class GitLatestHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var key = options["key"];

            var repository = GitHelper.GetRepositories().FirstOrDefault(r => r.Key.Equals(key));
            var branch = repository.DefaultBranch;

            if (options.ContainsKey("branch") && !string.IsNullOrEmpty(options["branch"]))
            {
                branch = options["branch"];
            }

            if (repository != null)
            {
                GitHelper.GetLatest(repository, branch);
            }
            
        }
    }
}
