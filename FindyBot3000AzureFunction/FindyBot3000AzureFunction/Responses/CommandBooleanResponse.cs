

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;

    public class CommandBooleanResponse : ICommandFlagResponse, ICommandResponse
    {
        public CommandBooleanResponse(string command, bool success)
        {
            this.Command = command;
            this.Success = success;
        }

        public string Command { get; private set; }

        public bool Success { get; set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
