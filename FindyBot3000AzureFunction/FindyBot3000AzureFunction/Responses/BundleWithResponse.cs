

namespace FindyBot3000.AzureFunction
{
    public class BundleWithResponse : CommandResponse, ICommandFlagResponse
    {
        public BundleWithResponse()
        {
            this.Command = Commands.BundleWith;
            this.Success = false;
        }

        public BundleWithResponse(bool success, string newItem, int quantity, string existingItem, int? row = null, int? col = null)
        {
            this.Command = Commands.BundleWith;
            this.Success = success;
            this.NewItem = newItem;
            this.Quantity = quantity;
            this.ExistingItem = existingItem;
            this.Row = row;
            this.Col = col;
        }

        public bool Success { get; set; }

        public string NewItem { get; set; }

        public int? Quantity { get; set; }

        public string ExistingItem { get; set; }

        public int? Row { get; set; }
         
        public int? Col { get; set; }

        public bool ShouldSerializeNewItem()
        {
            return this.NewItem != null;
        }

        public bool ShouldSerializeQuantity()
        {
            return this.Quantity != null;
        }

        public bool ShouldSerializeExistingItem()
        {
            return this.ExistingItem != null;
        }

        public bool ShouldSerializeRow()
        {
            return this.Row != null;
        }

        public bool ShouldSerializeCol()
        {
            return this.Col != null;
        }
    }
}
