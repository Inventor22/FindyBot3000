

namespace FindyBot3000.AzureFunction
{
    public class ShowAllBoxesResponse : CommandResponse, ICommandCountResponse
    {
        public ShowAllBoxesResponse()
        {
            this.Command = Commands.FindItem;
        }

        public ShowAllBoxesResponse(int count, string coords)
        {
            this.Command = Commands.FindItem;
            this.Count = count;
            this.Coords = coords;
        }

        public int Count { get; set; } = 0;

        public string Coords { get; set; } = string.Empty;
    }
}
