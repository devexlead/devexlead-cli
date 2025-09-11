using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Modules.Git.Helpers;

namespace DevExLead.Modules.Git.Handlers
{
    public class GitLatestHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var key = ParameterHelper.ReadStringParameter(options, "key");
            var branch = ParameterHelper.ReadStringParameter(options, "branch");

            if (string.IsNullOrEmpty(key))
            {
                var repositories = GitHelper.GetRepositories();
                foreach (var repository in repositories)
                {
                    GitHelper.GetLatest(repository, repository.DefaultBranch);
                }
            }
            else
            {
                var repository = GitHelper.GetRepositories().FirstOrDefault(r => r.Key.Equals(key));

                if (string.IsNullOrEmpty(branch))
                {
                    branch = repository.DefaultBranch;
                }

                if (repository != null)
                {
                    GitHelper.GetLatest(repository, branch);
                }
            }
        }
    }
}
