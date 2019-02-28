

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class FindItemResponse : ICommandCountResponse
    {
        public string Command { get { return Commands.FindItem; } }
        
        public int Count
        {
            get
            {
                return this.Result != null ? this.Result.Count : 0;
            }
        }

        public List<Item> Result { get; set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
