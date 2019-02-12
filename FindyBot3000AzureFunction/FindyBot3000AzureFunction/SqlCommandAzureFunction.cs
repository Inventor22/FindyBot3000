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
                        response = Command.InsertItem;
                        break;

                    case Command.RemoveItem:
                        response = Command.RemoveItem;
                        break;

                    case Command.AddTags:
                        response = Command.AddTags;
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
            var queryString = string.Format($"SELECT * FROM dbo.Item WHERE Item.Name LIKE '{item}'");

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

        public static string FindTags(string words, SqlConnection connection, ILogger log)
        {
            // Take a string of words: "Green motor driver"
            // Split it into an array of strings: string[] = { "Green", "motor", "driver" }
            // Format the words to be suited for the SQL-query: "'green','motor','driver'"
            string[] tags = words.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string formattedTags = string.Join(",", tags.Select(tag => string.Format("'{0}'", tag.Trim().ToLower())));
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

        // public static void InsertItem(string item, int quantity)
        // {

        // }

        // public static void RemoveItem(string item)
        // {

        // }

        // public static void AddTags(string item, List<string> tags)
        // {

        // }

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
