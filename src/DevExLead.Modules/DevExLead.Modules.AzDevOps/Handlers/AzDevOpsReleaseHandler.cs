using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using DevExLead.Integrations.Confluence;

namespace DevExLead.Modules.AzDevOps.Handlers
{
    public class AzDevOpsReleaseHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {

            var repositoryName = ParameterHelper.ReadStringParameter(options, "repository");
            var repository = UserStorageManager.GetUserStorage().Repositories.FirstOrDefault(r => r.Key == repositoryName);
            var organization = UserStorageManager.GetUserStorage().Applications.AzureDevOps.Organization;
            var project = UserStorageManager.GetUserStorage().Applications.AzureDevOps.Project;
            var personalAccessToken = UserStorageManager.GetUserStorage().Applications.AzureDevOps.PersonalAccessToken;

            if (repository != null)
            {
                var connector = new AzureDevOpsConnector(organization, project, personalAccessToken, true);

                foreach (var pipeline in repository.Pipelines)
                {

                }

            }
                
         
        }
    }
}
