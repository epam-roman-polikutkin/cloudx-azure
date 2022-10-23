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
    private const int RetryCount = 3;

    [FunctionName("ReserveOrderItemFunction")]
    public static async Task Run(
        [ServiceBusTrigger("send-order", Connection = "ServiceBusConnectionString")] string queueItem,
        ILogger log)
    {
        var isSent = false;
        var counter = 0;
        string responseMessage = null;
        while (!isSent && counter < RetryCount)
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
                responseMessage = $"ServiceBus triggered ReserveOrderItemFunction executed successfully. {name} was uploaded to blob storage";
                isSent = true;
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured during ReserveOrderItemFunction request processing");
            }
            
            counter++;
        }

        if (!isSent)
        {
            string logicAppsUri = Environment.GetEnvironmentVariable("LogicAppsUri");
            var email = new
            {
                to = "roman_polikutkin@epam.com",
                subject = "saving order request failed",
                body = $@"Dear admin, <br/> saving order request failed. <br/> Request info: {queueItem}"
            };

            using var client = new HttpClient();
            var strContent = JsonConvert.SerializeObject(email);
            var content = new StringContent(strContent);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await client.PostAsync(logicAppsUri, content);
            string resultContent = await result.Content.ReadAsStringAsync();

            log.LogInformation($"email with error was sent to admin group");
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
