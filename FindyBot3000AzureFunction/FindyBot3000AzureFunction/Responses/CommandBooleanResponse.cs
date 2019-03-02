

namespace FindyBot3000.AzureFunction
{
    public class CommandBooleanResponse : CommandResponse, ICommandFlagResponse
    {
        public CommandBooleanResponse(string command, bool success)
        {
            this.Command = command;
            this.Success = success;
        }

        public bool Success { get; set; }
    }
}
