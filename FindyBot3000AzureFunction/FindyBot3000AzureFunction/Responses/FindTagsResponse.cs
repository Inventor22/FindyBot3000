

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class FindTagsResponse : ICommandResponse
    {
        public FindTagsResponse(int tagsCount, List<int[]> result)
        {
            this.Tags = tagsCount;
            this.Result = result;
        }

        public string Command => Commands.FindTags;

        public int Count
        {
            get
            {
                return this.Result != null ? this.Result.Count : -1;
            }
        }

        public int Tags { get; set; }

        public List<int[]> Result { get; set; }
        
        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
