using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace FindyBot3000.AzureFunction
{
    public static class Command
    {
        public const string FindItem = "FindItem";
        public const string FindTags = "FindTags";
        public const string InsertItem = "InsertItem";
        public const string RemoveItem = "RemoveItem";
        public const string AddTags = "AddTags";
    }

    // Sql Table column names
    public class Dbo
    {
        public class Item
        {
            public static string Name = "Name";
            public static string Quantity = "Quantity";
            public static string Row = "Row";
            public static string Col = "Col";
            public static string SmallBox = "SmallBox";
            public static string DateCreated = "DateCreated";
            public static string LastUpdated = "LastUpdated";
        }
    }

    // This begs for a stateful azure function...
    public class MatrixModel
    {
        private const int TopRows = 8;
        private const int TopCols = 16;
        private const int BottomRows = 6;
        private const int BottomCols = 8;

        private bool[,] TopItems = new bool[TopRows, TopCols];
        private bool[,] BottomItems = new bool[BottomRows, BottomCols];

        public void AddItem(int row, int col)
        {
            if (row < 8)
            {
                this.TopItems[row, col] = true;
            }
            else if (row < 14)
            {
                this.BottomItems[row, col] = true;
            }
        }

        public (int, int) GetNextAvailableBox(bool isSmallBox)
        {
            if (isSmallBox)
            {
                for (int row = 0; row < TopRows; row++)
                {
                    for (int col = 0; col < TopCols; col++)
                    {
                        if (this.TopItems[row, col] == false)
                        {
                            this.TopItems[row, col] = true;
                            return (row, col);
                        }
                    }
                }
            }
            else
            {
                for (int row = 0; row < BottomRows; row++)
                {
                    for (int col = 0; col < BottomCols; col++)
                    {
                        if (this.TopItems[row, col] == false)
                        {
                            this.TopItems[row, col] = true;
                            return (row, col);
                        }
                    }
                }
            }

            return (-1, -1);
        }
    }

    public static class SqlCommandAzureFunction
    {
        [FunctionName("SqlCommandAzureFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string sqldb_connection = config.GetConnectionString("sqldb_connection");

            log.LogInformation(sqldb_connection);
            string response = string.Empty;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            using (SqlConnection connection = new SqlConnection(sqldb_connection))
            {
                connection.Open();

                LogHttpRequestBody(connection, requestBody);

                dynamic eventData = JsonConvert.DeserializeObject(requestBody);

                if (eventData == null)
                {
                    return new BadRequestObjectResult(
                        "Could not parse JSON input");
                }

                string unescapedJson = ((string)eventData.data).Replace(@"\", "");
                dynamic commands = JsonConvert.DeserializeObject(unescapedJson);

                if (commands == null || commands.command == null || commands.data == null)
                {
                    return new BadRequestObjectResult(
                        "Could not parse command JSON in data tag");
                }

                string command = commands.command;
                string data = commands.data;

                switch (command)
                {
                    case Command.FindItem:
                        response = FindItem(data, connection, log);
                        break;

                    case Command.FindTags:
                        response = FindTags(data, connection, log);
                        break;

                    case Command.InsertItem:
                        response = InsertItem(data, connection, log);
                        break;

                    case Command.RemoveItem:
                        response = RemoveItem(data, connection, log);
                        break;

                    case Command.AddTags:
                        response = AddTags(data, connection, log);
                        break;
                }

                try
                {
                    var requestResponseLogString = string.Format($"INSERT INTO dbo.Commands ([DateCreated], [Command], [DataIn], [DataOut]) VALUES (@param1, @param2, @param3, @param4)");
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandText = requestResponseLogString;
                        sqlCommand.Parameters.AddWithValue("@param1", DateTime.Now);
                        sqlCommand.Parameters.AddWithValue("@param2", command);
                        sqlCommand.Parameters.AddWithValue("@param3", data.ToLowerInvariant());
                        sqlCommand.Parameters.AddWithValue("@param4", response);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    log.LogInformation(ex.Message);
                }
            }

            return new OkObjectResult(response); // response
        }

        public static string FindItem(string item, SqlConnection connection, ILogger log)
        {
            var queryString = string.Format($"SELECT * FROM dbo.Item WHERE LOWER(Item.Name) LIKE '{item.ToLowerInvariant()}'");

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    List<object> jsonObjects = new List<object>();
                    while (reader.Read())
                    {
                        jsonObjects.Add(
                            new
                            {
                                Name = (string)reader[Dbo.Item.Name],
                                Quantity = (int)reader[Dbo.Item.Quantity],
                                Row = (int)reader[Dbo.Item.Row],
                                Column = (int)reader[Dbo.Item.Col]
                            });
                    }

                    var response = new
                    {
                        Command = Command.FindItem,
                        Count = jsonObjects.Count,
                        Result = jsonObjects
                    };

                    string jsonQueryResponse = JsonConvert.SerializeObject(response);
                    log.LogInformation(jsonQueryResponse);

                    return jsonQueryResponse;
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }
        }

        public static string FindTags(string words, SqlConnection connection, ILogger log)
        {
            // Take a string of words: "Green motor driver"
            // Split it into an array of strings: string[] = { "Green", "motor", "driver" }
            // Format the words to be suited for the SQL-query: "'green','motor','driver'"
            string[] tags = words.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string formattedTags = string.Join(",", tags.Select(tag => string.Format("'{0}'", tag.Trim().ToLowerInvariant())));
            log.LogInformation(formattedTags);

            var queryString = string.Format(
$@"SELECT i.Name, i.Quantity, i.Row, i.Col, t.TagsMatched
FROM dbo.Item i JOIN
(
    SELECT Name, COUNT(Name) TagsMatched
    FROM dbo.Tags
    WHERE Tag IN({formattedTags})
    GROUP BY Name
) t ON i.Name = t.Name
ORDER BY t.TagsMatched DESC");

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    List<object> jsonObjects = new List<object>();
                    while (reader.Read())
                    {
                        jsonObjects.Add(
                            new
                            {
                                Name = (string)reader["Name"],
                                Quantity = (int)reader["Quantity"],
                                Row = (int)reader["Row"],
                                Column = (int)reader["Col"],
                                TagsMatched = (int)reader["TagsMatched"]
                            });
                    }

                    var response = new
                    {
                        Command = Command.FindTags,
                        Count = jsonObjects.Count,
                        Result = jsonObjects
                    };

                    string jsonQueryResponse = JsonConvert.SerializeObject(response);
                    log.LogInformation(jsonQueryResponse);

                    return jsonQueryResponse;
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }
        }

        // 1. Check if item exists, if yes, return the box it's in.
        // 2. Query for currently used boxes, find an empty box if one exists, and return it's location
        public static string InsertItem(string itemAndQuantity, SqlConnection connection, ILogger log)
        {
            dynamic itemAndQuantityJson = JsonConvert.DeserializeObject(itemAndQuantity);
            string item = itemAndQuantityJson["Item"];
            int quantity = itemAndQuantityJson["Quantity"];
            bool isSmallBox = itemAndQuantityJson["IsSmallBox"];

            string itemLower = item.ToLowerInvariant();
            IEnumerable<string> tags = itemLower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim());
            
            var checkIfExistsQuery = $@"SELECT Name FROM dbo.Item WHERE LOWER(Item.Name) LIKE {itemLower}";

            using (SqlCommand command = new SqlCommand(checkIfExistsQuery, connection))
            {
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    try
                    {
                        List<object> jsonObjects = new List<object>();
                        while (reader.Read())
                        {
                            jsonObjects.Add(
                                new
                                {
                                    Name = (string)reader["Name"],
                                    Quantity = (int)reader["Quantity"],
                                    Row = (int)reader["Row"],
                                    Column = (int)reader["Col"]
                                });
                        }

                        var response = new
                        {
                            Command = Command.FindItem,
                            Count = jsonObjects.Count,
                            Result = jsonObjects
                        };

                        string jsonQueryResponse = JsonConvert.SerializeObject(response);
                        log.LogInformation(jsonQueryResponse);

                        return jsonQueryResponse;
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }
                }
            }

            // Item doesn't exist; insert.
            // Find existing boxes
            var sqlAllConsumedBoxes = string.Format("SELECT ROW, COL FROM dbo.Item");
            MatrixModel matrix = new MatrixModel();

            using (SqlCommand command = new SqlCommand(checkIfExistsQuery, connection))
            {
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    try
                    {
                        List<object> jsonObjects = new List<object>();
                        while (reader.Read())
                        {
                            matrix.AddItem((int)reader[Dbo.Item.Row], (int)reader[Dbo.Item.Col]);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }

            var (row, col) = matrix.GetNextAvailableBox(isSmallBox);

            if (row == -1 && col == -1)
            {
                return $"No {(isSmallBox ? "Small" : "Large")} boxes left!";
            }

            var sqlInsertString = string.Format($@"
INSERT INTO dbo.Item([Name], [Quantity], [Row], [Column], [SmallBox], [DateCreated], [LastUpdated])
VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7)");

            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = connection;
                sqlCommand.CommandText = sqlInsertString;
                sqlCommand.Parameters.AddWithValue("@param1", item);
                sqlCommand.Parameters.AddWithValue("@param2", quantity);
                sqlCommand.Parameters.AddWithValue("@param3", row);
                sqlCommand.Parameters.AddWithValue("@param4", col);
                sqlCommand.Parameters.AddWithValue("@param5", isSmallBox);
                sqlCommand.Parameters.AddWithValue("@param6", DateTime.UtcNow);
                sqlCommand.Parameters.AddWithValue("@param7", DateTime.UtcNow);
                sqlCommand.ExecuteNonQuery();
            }

            return Command.InsertItem;
        }

        public static string RemoveItem(string item, SqlConnection connection, ILogger log)
        {
            return Command.RemoveItem;
        }

        public static string AddTags(string item, SqlConnection connection, ILogger log)
        {
            return Command.AddTags;
        }

        public static void LogHttpRequestBody(SqlConnection connection, string requestBody)
        {
            var httpRequestString = $"INSERT INTO dbo.HttpRequests ([HttpRequestBody], [DateCreated]) VALUES (@param1, @param2)";
            using (SqlCommand sqlCommand2 = new SqlCommand())
            {
                sqlCommand2.Connection = connection;
                sqlCommand2.CommandText = httpRequestString;
                sqlCommand2.Parameters.AddWithValue("@param1", requestBody);
                sqlCommand2.Parameters.AddWithValue("@param2", DateTime.Now);
                sqlCommand2.ExecuteNonQuery();
            }
        }
    }
}
