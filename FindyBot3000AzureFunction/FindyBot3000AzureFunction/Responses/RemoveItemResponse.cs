using System;
using System.Collections.Generic;
using System.Text;

namespace FindyBot3000.AzureFunction
{
    public class RemoveItemResponse : CommandResponse, ICommandFlagResponse
    {
        public RemoveItemResponse(bool success, string itemForRemoval)
        {
            this.Command = Commands.RemoveItem;
            this.Success = false;
            this.ItemForRemoval = itemForRemoval;
        }

        public RemoveItemResponse(bool success, string itemForRemoval, int? row, int? col)
        {
            this.Command = Commands.RemoveItem;
            this.Success = success;
            this.ItemForRemoval = itemForRemoval;
            this.Row = row;
            this.Col = col;
        }

        public bool Success { get; set; }

        public string ItemForRemoval { get; set; }

        public int? Row { get; set; }

        public int? Col { get; set; }
    }
}
