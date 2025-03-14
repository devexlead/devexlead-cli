namespace DevExLead.Core
{
    public interface ICommandHandler
    {
        Task ExecuteAsync(Dictionary<string, string> options);
    }
}
