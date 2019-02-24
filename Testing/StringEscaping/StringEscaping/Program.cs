using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringEscaping
{
    class Program
    {
        static void Main(string[] args)
        {
            string data = "\"{\\\"Command\\\":\\\"FindItem\\\",\\\"Count\\\":1,\\\"Result\\\":[{\\\"Name\\\":\\\"Yellow LED\\\",\\\"Quantity\\\":43,\\\"Row\\\":0,\\\"Column\\\":0}]}\"";

            // Prints exactly (quotations included): "{\"Command\":\"FindItem\",\"Count\":1,\"Result\":[{\"Name\":\"Yellow LED\",\"Quantity\":43,\"Row\":0,\"Column\":0}]}"
            Console.WriteLine(data);

            int dataLen = data.Length;
            char[] msg = new char[dataLen];
            int j = 0;
            for (int i = 1; i < dataLen - 1; i++)
            {
                if (data[i] == '\\') continue;
                msg[j++] = data[i];
            }
            msg[j] = '\0'; // Terminate the string

            Console.WriteLine(msg);
            Console.ReadKey();
        }
    }
}
