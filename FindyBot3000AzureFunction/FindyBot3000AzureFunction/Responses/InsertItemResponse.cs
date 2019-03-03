

namespace FindyBot3000.AzureFunction
{
    public class InsertItemResponse : CommandResponse, ICommandFlagResponse
    {
        public InsertItemResponse(bool success)
        {
            this.Command = Commands.InsertItem;
            this.Success = success;
        }

        public InsertItemResponse(bool success, int row, int col, string name = "")
        {
            this.Command = Commands.InsertItem;
            this.Success = success;
            this.Row = row;
            this.Col = col;
            this.Name = name;
        }

        public bool Success { get; set; }

        public string Name { get; set; }

        public string NameKey
        {
            get
            {
                return string.IsNullOrEmpty(this.Name)
                       ? null
                       : QueryHelper.Instance.SingularizeAndLower(this.Name);
            }
        }

        public int? Row { get; set; }

        public int? Col { get; set; }
    }
}
