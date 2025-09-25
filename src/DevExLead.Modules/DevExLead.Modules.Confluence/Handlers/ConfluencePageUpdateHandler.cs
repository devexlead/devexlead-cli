using DevEx.Integrations.Confluence;
using DevEx.Integrations.GitHub.Model;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;

namespace DevExLead.Modules.Confluence.Handlers
{
    public class ConfluencePageUpdateHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var pageId = ParameterHelper.ReadStringParameter(options, "pageId");
            var filename = ParameterHelper.ReadStringParameter(options, "filename");
            var comment = ParameterHelper.ReadStringParameter(options, "comment");

            bool isLoggingEnabled = UserStorageManager.GetUserStorage().IsLoggingEnabled;
            var baseUrl = UserStorageManager.GetDecryptedValue("Confluence:BaseUrl");
            var user = UserStorageManager.GetDecryptedValue("Confluence:User");
            var key = UserStorageManager.GetDecryptedValue("Confluence:Key");

            var confluenceConnector = new ConfluenceConnector(baseUrl, user, key, isLoggingEnabled);
            await confluenceConnector.UpdatePageAsync(pageId, "", comment);
        }
    }
}
