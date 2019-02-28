

namespace FindyBot3000.AzureFunction
{
    public interface ICommandFlagResponse : ICommandResponse
    {
        bool Success { get; set; }
    }
}
