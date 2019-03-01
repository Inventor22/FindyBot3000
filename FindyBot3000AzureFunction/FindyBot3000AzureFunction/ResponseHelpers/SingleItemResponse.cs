

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class SingleItemResponse : ICommandItemResponse, ICommandResponse
    {
        public SingleItemResponse(string command)
        {
            this.Command = command;
        }

        public SingleItemResponse(string command, List<Item> result)
        {
            this.Command = command;
            this.Result = result;
        }

        public string Command { get; private set; }

        public int Count { get { return this.Result != null ? this.Result.Count : -1; } }

        public List<Item> Result { get; set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
