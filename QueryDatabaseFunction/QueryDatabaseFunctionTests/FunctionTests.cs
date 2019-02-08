using FindyBot3000.AzureFunction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace FindyBot3000.Tests
{
    public class FunctionTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        private static Dictionary<string, StringValues> CreateDictionary(string key, string value)
        {
            var qs = new Dictionary<string, StringValues>
            {
                { key, value }
            };
            return qs;
        }

        public static Stream GenerateStringStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        [Fact]
        public async void Http_trigger_should_return_known_string()
        {
            var particlePhotonData = new
            {
                name = "databaseQueryEvent",
                data = "{\"command\":\"find\", \"data\":\"BB Battery\"}",
                ttl = 60,
                published_at = "2019-01-27T02:23:55.127Z",
                coreid = "3e0023000447343339373536"
            };

            string payload = JsonConvert.SerializeObject(particlePhotonData);
            
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStringStream(payload)
            };
                        
            var response = (OkObjectResult) await QueryDatabaseFunction.Run(request, logger);
            Assert.Equal("Command8", response.Value);
        }

        [Fact]
        public void Timer_should_log_message()
        {
            var logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            QueryDatabaseFunction.Run(null, logger);
            var msg = logger.Logs[0];
            Assert.Contains("C# Timer trigger function executed at", msg);
        }
    }
}
