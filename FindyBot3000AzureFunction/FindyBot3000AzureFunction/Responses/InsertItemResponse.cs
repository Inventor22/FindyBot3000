

namespace FindyBot3000.AzureFunction
{
    public class InsertItemResponse : CommandResponse, ICommandFlagResponse
    {
        public InsertItemResponse(bool success)
        {
            this.Command = Commands.InsertItem;
            this.Success = success;
        }

        public InsertItemResponse(bool success, int row, int col)
        {
            this.Command = Commands.InsertItem;
            this.Success = success;
            this.Row = row;
            this.Col = col;
        }

        public bool Success { get; set; }

        public int? Row { get; set; }

        public int? Col { get; set; }
    }
}
