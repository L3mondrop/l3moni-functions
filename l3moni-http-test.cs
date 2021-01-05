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
            var mongoClient = SetupMongo();
            var database = SetupDB("test", mongoClient);
            var collection = GetCollection("l3moni",database);

            var document = new BsonDocument {
                { "id", ""},
                { "name", "demoukkeli"},
                { "skill", "none"}
            };

           // collection.InsertOne(document);

            

            log.LogInformation("C# HTTP trigger function processed a request.");

            string value = req.Query["value"];

            string json = req.Query["json"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            value = value ?? data?.value;

            json = json ?? data?.json;

            // Create
            if (req.Method == HttpMethods.Post) {

                System.Diagnostics.Debug.WriteLine("HttpPost received");
                try {
                    log.LogInformation(InsertJson(json,collection));
                }
                catch (Exception e) {
                    log.LogInformation(e.Message);
                }
            }

            // Read
            else if (req.Method == HttpMethods.Get) {
                System.Diagnostics.Debug.WriteLine("HttpGet received");
            }
            // Update
            else if (req.Method == HttpMethods.Put) {
                System.Diagnostics.Debug.WriteLine("HttpPut received");
            }
            // Delete
            else if (req.Method == HttpMethods.Delete) {
                System.Diagnostics.Debug.WriteLine("HttpDelete received");
            }

            

            string responseMessage = string.IsNullOrEmpty(value)
                ? "Please provide a city as a value ie. ?value=Espoo"
                : $"Weather forecast today in {value} is ";

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

        public static string InsertJson(string json, IMongoCollection<BsonDocument> collection) {

            BsonDocument result;
            bool success = BsonDocument.TryParse(json, out result);

            if(success) {
                collection.InsertOne(result);
                return ("Inserted object in to: " + collection.CollectionNamespace.ToString());
            }
            else {
                return "JSON Parse failed";
            }
        }

    }
}
