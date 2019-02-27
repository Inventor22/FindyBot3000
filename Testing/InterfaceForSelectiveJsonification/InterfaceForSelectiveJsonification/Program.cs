using FindyBot3000.AzureFunction;
using System;
using System.Collections.Generic;

namespace InterfaceForSelectiveJsonification
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            FindItemResponse resp = new FindItemResponse
            {
                Result = new List<Item>()
                {
                    new Item
                    {
                        Name = "Name",
                        Quantity = 1,
                        Row = 2,
                        Col = 3
                    }
                }
            };

            string response = resp.ToJsonString(true);
            Console.WriteLine(response);

            Console.ReadKey();
        }
    }
}
