

namespace FindyBot3000.AzureFunction
{
    using Newtonsoft.Json;

    public class InsertItemResponse : ICommandFlagResponse, ICommandResponse
    {
        public InsertItemResponse(bool success)
        {
            this.Success = success;
        }

        public InsertItemResponse(bool success, int row, int col)
        {
            this.Success = success;
            this.Row = row;
            this.Col = col;
        }

        public string Command => Commands.InsertItem;

        public bool Success { get; set; }

        public int? Row { get; set; }

        public int? Col { get; set; }

        public string ToJsonString(bool indent = false)
        {
            return JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }
    }
}
