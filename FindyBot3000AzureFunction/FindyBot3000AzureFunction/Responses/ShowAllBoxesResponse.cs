

namespace FindyBot3000.AzureFunction
{
    public class ShowAllBoxesResponse : CommandResponse, ICommandCountResponse
    {
        public ShowAllBoxesResponse()
        {
            this.Command = Commands.ShowAllBoxes;
            this.Count = -1;
        }

        public ShowAllBoxesResponse(int count, string coords)
        {
            this.Command = Commands.ShowAllBoxes;
            this.Count = count;
            this.Coords = coords;
        }

        public int Count { get; set; }

        public string Coords { get; set; }
    }
}
