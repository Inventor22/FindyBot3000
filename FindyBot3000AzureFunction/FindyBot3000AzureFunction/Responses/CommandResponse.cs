


namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;

    public abstract class CommandResponse : ICommandResponse
    {
        [JsonProperty(Order = -2)]
        public string Command { get; protected set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
