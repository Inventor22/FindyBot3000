

namespace FindyBot3000.AzureFunction
{
    using System.Collections.Generic;

    public class SingleItemResponse : CommandResponse, ICommandItemResponse
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

        public int Count { get { return this.Result != null ? this.Result.Count : -1; } }

        public List<Item> Result { get; set; }
    }
}
