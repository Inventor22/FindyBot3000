#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Text;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");
    
    var sqldb_connection = Environment.GetEnvironmentVariable("sqldb_connection");
    log.LogInformation(sqldb_connection);
    string response = string.Empty;

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    log.LogInformation(requestBody);

    using (SqlConnection connection = new SqlConnection(sqldb_connection))
    {
        connection.Open();

        LogHttpRequestBody(connection, requestBody);
        
        dynamic eventData = JsonConvert.DeserializeObject(requestBody);

        if (eventData == null) {
            return new BadRequestObjectResult(
                "Could not parse JSON input");
        }

        string unescapedJson = ((string)eventData.data).Replace(@"\", "");
        dynamic commands = JsonConvert.DeserializeObject(unescapedJson);

        if (commands == null || commands.command == null || commands.data == null) {
            return new BadRequestObjectResult(
                "Could not parse command JSON in data tag");
        }

        string command = commands.command;
        string data = commands.data;  

        switch (command.ToLowerInvariant())
        {
            case "find":
            response = FindItem(data, connection, log);
            break;

            case "insert":
            break;

            case "remove":
            break;

            case "addtags":
            break;
        }

        try 
        {
            var requestResponseLogString = string.Format($"INSERT INTO dbo.Commands ([DateCreated], [Command], [DataIn], [DataOut]) VALUES (@param1, @param2, @param3, @param4)");
            using(SqlCommand sqlCommand = new SqlCommand())
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

    return (ActionResult)new OkObjectResult("Command8"); // response
}

public static void LogHttpRequestBody(SqlConnection connection, string requestBody)
{
    var httpRequestString = $"INSERT INTO dbo.HttpRequests ([HttpRequestBody], [DateCreated]) VALUES (@param1, @param2)";
    using(SqlCommand sqlCommand2 = new SqlCommand())
    {
        sqlCommand2.Connection = connection;
        sqlCommand2.CommandText = httpRequestString;      
        sqlCommand2.Parameters.AddWithValue("@param1", requestBody);
        sqlCommand2.Parameters.AddWithValue("@param2", DateTime.Now);
        sqlCommand2.ExecuteNonQuery();
    }
}

public static string FindItem(string item, SqlConnection connection, ILogger log)
{
    var queryString = string.Format($"SELECT * FROM dbo.Item WHERE Item.Name LIKE '{item}'"); // $"SELECT * FROM dbo.Item"

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
                        Name = (string) reader["Name"],
                        Quantity = (int) reader["Quantity"],
                        Row = (int) reader["Row"],
                        Column = (int) reader["Column"]
                    });
            }

            string jsonQueryResponse = JsonConvert.SerializeObject(jsonObjects);
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
    string[] tags = string.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    log.LogInformation(string.Join("|", tags));

    var queryString = string.Format($"SELECT Name FROM dbo.Tags WHERE Item.Name LIKE '{item}'"); // $"SELECT * FROM dbo.Item"

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
                        Name = (string) reader["Name"],
                        Quantity = (int) reader["Quantity"],
                        Row = (int) reader["Row"],
                        Column = (int) reader["Column"]
                    });
            }

            string jsonQueryResponse = JsonConvert.SerializeObject(jsonObjects);
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