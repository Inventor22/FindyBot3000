

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class FindItemResponse : ICommandItemResponse
    {
        public FindItemResponse() { }

        public FindItemResponse(List<Item> result)
        {
            this.Result = result;
        }

        public string Command { get { return Commands.FindItem; } }
        
        public int Count
        {
            get
            {
                return this.Result != null ? this.Result.Count : -1;
            }
        }

        public List<Item> Result { get; set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
