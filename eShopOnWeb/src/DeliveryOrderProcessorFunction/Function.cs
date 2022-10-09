using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.eShopWeb.ApplicationCore.Entities.Requests;

namespace DeliveryOrderProcessorFunction
{
    public static class Function
    {
        private const string DatabaseId = "ProcessedOrders";
        private const string ContainerId = "Orders";

        private static CosmosClient cosmosClient;
        private static Database database;
        private static Container container;

        static Function()
        {
            string endpointUri = Environment.GetEnvironmentVariable("EndpointUri");
            string primaryKey = Environment.GetEnvironmentVariable("PrimaryKey");

            cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "DeliveryOrderProcessorFunction" });
        }

        [FunctionName("DeliveryOrderProcessorFunction")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
                container = await database.CreateContainerIfNotExistsAsync(ContainerId, "/shipToAddress/city");

                var result = GetRequestBody(req);
                var order = JsonConvert.DeserializeObject<OrderRequest>(result);
                ItemResponse<OrderRequest> response =
                    await container.CreateItemAsync(order, new PartitionKey(order.ShipToAddress.City));

                var responseMessage = $"HTTP triggered DeliveryOrderProcessorFunction executed successfully. Order (id = {order.Id}) was uploaded to delivery storage";

                return new OkObjectResult(responseMessage);
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured during ReserveOrderItemFunction request processing");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }

        private static string GetRequestBody(HttpRequest req)
        {
            var bodyStream = new StreamReader(req.Body);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();

            return bodyText;
        }
    }
}
