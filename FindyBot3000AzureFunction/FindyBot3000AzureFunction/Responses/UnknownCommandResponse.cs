

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;

    public class UnknownCommandResponse : ICommandResponse
    {
        public UnknownCommandResponse(string unknownCommand)
        {
            this.UnknownCommand = unknownCommand;
        }

        public string Command { get { return "UnknownCommand"; } }

        public string UnknownCommand { get; set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
