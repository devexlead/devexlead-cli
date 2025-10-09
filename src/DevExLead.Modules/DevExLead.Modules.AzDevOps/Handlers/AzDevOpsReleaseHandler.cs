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

            var azDevOpsConfiguration = UserStorageManager.GetUserStorage().Applications.AzureDevOps;
            var organization = azDevOpsConfiguration.Organization;
            var project = azDevOpsConfiguration.Project;
            var personalAccessToken = UserStorageManager.GetDecryptedValue("AzureDevOps:PersonalAccessToken");

            if (repository != null && repository.Pipelines !=null)
            {
                var connector = new AzureDevOpsConnector(organization, project, personalAccessToken, true);
                var pipelines = await connector.FetchPipelines();

                foreach (var pipelineName in repository.Pipelines)
                {
                    var pipeline = pipelines.FirstOrDefault(p => p.name.Equals(pipelineName));
                    
                    //statusFilter = notStarted,completed,inProgress,cancelling,postponed,all
                    var builds = await connector.FetchBuilds(pipeline.id, "refs/heads/main", "inProgress,completed");
                    
                    var runs = await connector.FetchRuns(pipeline.id);
                    foreach (var run in runs)
                    {
                        var build = builds.FirstOrDefault(b => b.buildNumber == run.name);
                        Console.WriteLine($"{run.id} - {run.state} - {build.triggerInfo.cimessage} - {build.triggerInfo.cisourceBranch} - {build.triggerInfo.cisourceSha}");
                        //await connector.RunBuild(pipeline.id, run.id, build.triggerInfo.cisourceBranch, build.triggerInfo.cisourceSha, azDevOpsConfiguration.StagesToSkip);
                    }
                }
                
            }
                
         
        }
    }
}
