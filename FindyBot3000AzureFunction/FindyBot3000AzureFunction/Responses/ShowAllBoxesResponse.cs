

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;

    public class ShowAllBoxesResponse : ICommandResponse
    {
        public ShowAllBoxesResponse() { }

        public ShowAllBoxesResponse(int count, string coords)
        {
            this.Count = count;
            this.Coords = coords;
        }

        public string Command { get { return Commands.FindItem; } }

        public int Count { get; set; } = 0;

        public string Coords { get; set; } = string.Empty;

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
