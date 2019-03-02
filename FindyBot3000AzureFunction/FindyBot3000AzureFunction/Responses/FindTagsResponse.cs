

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class FindTagsResponse : CommandResponse
    {
        public FindTagsResponse(int tagsCount, List<int[]> result)
        {
            this.Command = Commands.FindTags;
            this.Tags = tagsCount;
            this.Result = result;
        }
        
        public int Count
        {
            get
            {
                return this.Result != null ? this.Result.Count : -1;
            }
        }

        public int Tags { get; set; }

        public List<int[]> Result { get; set; }
    }
}
