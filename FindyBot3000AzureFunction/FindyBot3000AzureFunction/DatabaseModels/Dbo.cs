
namespace FindyBot3000.AzureFunction
{
    // Sql Table column names
    public class Dbo
    {
        public class Items
        {
            public const string Name = "Name";
            public const string Quantity = "Quantity";
            public const string Row = "Row";
            public const string Col = "Col";
            public const string IsSmallBox = "IsSmallBox";
            public const string DateCreated = "DateCreated";
            public const string LastUpdated = "LastUpdated";
        }

        public class Tags
        {
            public const string Name = "Name";
            public const string Tag = "Tag";
        }

        public class Commando
        {
            public const string DateCreated = "DateCreated";
            public const string Command = "Command";
            public const string DataIn = "DataIn";
            public const string DataOut = "DataOut";
        }

        public class HttpRequests
        {
            public const string HttpRequestBody = "HttpRequestBody";
            public const string DateCreated = "DateCreated";
        }
    }
}
