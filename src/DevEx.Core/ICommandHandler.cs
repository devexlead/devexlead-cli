namespace DevEx.Core
{
    public interface ICommandHandler
    {
        void Execute(Dictionary<string, string> options);
    }
}
