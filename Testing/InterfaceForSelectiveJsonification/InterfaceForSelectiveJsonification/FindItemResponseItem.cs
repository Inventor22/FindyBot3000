using System;
using System.Collections.Generic;
using System.Text;

namespace FindyBot3000.AzureFunction
{
    public class FindItemResponseItem : Item
    {
        new string Name { get; set; }
        new int Quantity { get; set; }
        new int Row { get; set; }
        new int Col { get; set; }
    }
}
