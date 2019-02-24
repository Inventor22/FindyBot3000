using Newtonsoft.Json;
using System;

namespace ValidateJson
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string msg = "{\"Command\":\"FindTags\",\"Count\":2,\"Result\":[{\"Name\":\"Yellow LED\",\"Quantity\":43,\"Row\":0,\"Col\":4,\"TagsMatched\":2,\"Confidence\":1.0},{\"Name\":\"Green LED\",\"Quantity\":22,\"Row\":0,\"Col\":2,\"TagsMatched\":1,\"Confidence\":0.5}]}";
            dynamic a = JsonConvert.DeserializeObject(msg);

            Console.WriteLine(a.Command);
            Console.ReadKey();
        }
    }
}
