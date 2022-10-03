using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace OrderItemsReserver;

public static class ReserveOrderItemFunction
{
    [FunctionName("ReserveOrderItemFunction")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        try
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("StorageContainerName");

            var orderId = req.Query["orderId"];
            var name = $"OrderRequest-{orderId}.json";

            var blobClient = new BlobContainerClient(connection, containerName);
            var blob = blobClient.GetBlobClient(name);

            await blob.UploadAsync(req.Body);

            string responseMessage = $"HTTP triggered ReserveOrderItemFunction executed successfully. {name} was uploaded to blob storage";

            return new OkObjectResult(responseMessage);
        }
        catch (Exception e)
        {
            log.LogError(e, "Exception occured during ReserveOrderItemFunction request processing");

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
