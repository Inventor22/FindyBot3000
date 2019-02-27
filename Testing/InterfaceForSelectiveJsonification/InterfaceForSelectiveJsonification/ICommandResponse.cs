using System;
using System.Collections.Generic;
using System.Text;

namespace FindyBot3000.AzureFunction
{
    public interface ICommandResponse
    {
        string Command { get; }

        bool Success { get; set; }

        string ToJsonString(bool indent = false);
    }
}
