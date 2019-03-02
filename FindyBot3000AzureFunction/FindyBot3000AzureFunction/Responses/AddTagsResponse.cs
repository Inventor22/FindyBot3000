

namespace FindyBot3000.AzureFunction
{
    public class AddTagsResponse : CommandResponse, ICommandCountResponse, ICommandFlagResponse
    {
        public AddTagsResponse(bool success, int count)
        {
            this.Command = Commands.AddTags;
            this.Success = success;
            this.Count = count;
        }
        
        public int Count { get; set; }

        public bool Success { get; set; }
    }
}
