

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class FindItemResponse : CommandResponse, ICommandItemResponse
    {
        public FindItemResponse()
        {
            this.Command = Commands.FindItem;
        }

        public FindItemResponse(List<Item> result)
        {
            this.Result = result;
            this.Command = Commands.FindItem;
        }
                
        public int Count
        {
            get
            {
                return this.Result != null ? this.Result.Count : -1;
            }
        }

        public List<Item> Result { get; set; }
    }
}
