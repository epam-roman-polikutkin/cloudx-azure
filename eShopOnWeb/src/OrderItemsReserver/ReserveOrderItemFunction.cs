using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Microsoft.eShopWeb.ApplicationCore.Entities.Requests;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OrderItemsReserver;

public static class ReserveOrderItemFunction
{
    [FunctionName("ReserveOrderItemFunction")]
    public static async Task Run(
        [ServiceBusTrigger("send-order", Connection = "ServiceBusConnectionString")] string queueItem,
        ILogger log)
    {
        try
        {
            string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("StorageContainerName");

            var orderRequest = JsonConvert.DeserializeObject<OrderRequest>(queueItem);
            var name = $"OrderRequest-{orderRequest.Id}.json";

            var blobClient = new BlobContainerClient(connection, containerName);
            var blob = blobClient.GetBlobClient(name);

            var stream = GenerateStreamFromString(queueItem);

            await blob.UploadAsync(stream);

            log.LogInformation($"C# ServiceBus queue trigger function processed message: {queueItem}");
        }
        catch (Exception e)
        {
            log.LogError(e, "Exception occured during ReserveOrderItemFunction request processing");
            throw;
        }
    }

    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;

        return stream;
    }
}
