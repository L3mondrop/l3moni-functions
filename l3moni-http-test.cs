using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Diagnostics;
//using l3moni.Function;
using System.Security.Authentication;


namespace l3moni.Function // Company namespace
{
    public static class l3moni_http_test
    {
        [FunctionName("l3moni_http_test")] // Function name
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = null)] HttpRequest req,
            ILogger log)
        {   
            // Replace with ENV Variables in local.settings.json / Portal App Settings
            var databasename = System.Environment.GetEnvironmentVariable("DEFAULT_DATABASE_NAME");
            var collectionname = System.Environment.GetEnvironmentVariable("DEFAULT_COLLECTION_NAME");
            
            // Setup and test MongoDB / CosmosDB connection
            var mongoClient = SetupMongo();
            var database = SetupDB(databasename, mongoClient);
            var collection = GetCollection(collectionname,database);

            // Check for req headers (application/json)
            string value = req.Query["value"];

            string json = req.Query["json"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Converting JSON request to a usable format
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            value = value ?? data?.value;

            json = json ?? data?.json;

            // Log incoming request
            log.LogInformation(requestBody);

            string responseMessage = "OK";

            // Create
            if (req.Method == HttpMethods.Post) {

                System.Diagnostics.Debug.WriteLine("HttpPost received");
                try {
                    InsertJson(json,collection, log);
                }
                catch (Exception e) {
                    log.LogInformation(e.Message);
                    responseMessage = "Http.Get Success but with error: " + e.Message;
                }
            }

            // Read
            else if (req.Method == HttpMethods.Get) {
                System.Diagnostics.Debug.WriteLine("HttpGet received");
                responseMessage = "Http.Get Success";
            }
            // Update
            else if (req.Method == HttpMethods.Put) {
                System.Diagnostics.Debug.WriteLine("HttpPut received");
                responseMessage = "Http.Put Success";
            }
            // Delete
            else if (req.Method == HttpMethods.Delete) {
                System.Diagnostics.Debug.WriteLine("HttpDelete received");
                responseMessage = "Http.Delete Success";
            }

            return new OkObjectResult(responseMessage);
        }

// Setting up MongoClient
        public static MongoClient SetupMongo() {

            
            string connectionString = System.Environment.GetEnvironmentVariable("MONGODB_STRING");
            
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));

            settings.SslSettings = new SslSettings() 
            { 
                EnabledSslProtocols = SslProtocols.Tls12 
            };

            System.Diagnostics.Debug.WriteLine("MongoDB init ");
            return new MongoClient(settings);

        }

// Setting up right database with dbname string
        public static IMongoDatabase SetupDB(string dbname, MongoClient client) {

            System.Diagnostics.Debug.WriteLine("Database init: " + dbname);
             return client.GetDatabase(dbname);

        }

// Setting up a right collection to be used
        public static IMongoCollection<BsonDocument> GetCollection(string collectionName, IMongoDatabase database) {

            System.Diagnostics.Debug.WriteLine("Collection init: " + collectionName);
            return database.GetCollection<BsonDocument>(collectionName);
        }

        public static void InsertJson(string json, IMongoCollection<BsonDocument> collection, ILogger log) {

            BsonDocument result;
            
            // Refactor this with try catch logic
            try {
                bool success = BsonDocument.TryParse(json, out result);
                if(success) {
                collection.InsertOne(result);
                }
            }
            catch (Exception e) {
                log.LogError("InsertJson error: " + e.Message);
            }
        }

    }
}
