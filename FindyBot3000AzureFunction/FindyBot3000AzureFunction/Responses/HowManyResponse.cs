

namespace FindyBot3000.AzureFunction
{
    public class HowManyResponse : CommandResponse, ICommandFlagResponse
    {
        public HowManyResponse(bool success, string name)
        {
            this.Command = Commands.HowMany;
            this.Success = false;
            this.Name = name;
        }

        public HowManyResponse(bool success, string name, int? quantity, int? row, int? col)
        {
            this.Command = Commands.HowMany;
            this.Success = success;
            this.Name = name;
            this.Quantity = quantity;
            this.Row = row;
            this.Col = col;
        }

        public bool Success { get; set; }

        public string Name { get; set; }

        public int? Quantity { get; set; }

        public int? Row { get; set; }

        public int? Col { get; set; }
        
    }
}
