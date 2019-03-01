using System;
using System.Collections.Generic;
using System.Text;

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;

    public class AddTagsResponse : ICommandCountResponse, ICommandFlagResponse, ICommandResponse
    {
        public AddTagsResponse(bool success, int count)
        {
            this.Success = success;
            this.Count = count;
        }

        public string Command => Commands.AddTags;

        public int Count { get; set; }

        public bool Success { get; set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
