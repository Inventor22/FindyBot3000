using System;
using System.Collections.Generic;
using System.Text;

namespace FindyBot3000.AzureFunction
{
    public interface ICommandItemResponse
    {
        int Count { get; }

        List<Item> Result { get; set; }
    }
}
