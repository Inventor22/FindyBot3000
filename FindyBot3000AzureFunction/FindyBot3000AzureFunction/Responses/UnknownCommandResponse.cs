

namespace FindyBot3000.AzureFunction
{
    public class UnknownCommandResponse : CommandResponse
    {
        public UnknownCommandResponse(string unknownCommand)
        {
            this.Command = Commands.UnknownCommand;
            this.UnknownCommand = unknownCommand;
        }

        public string UnknownCommand { get; set; }
    }
}
