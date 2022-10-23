using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using BlazorShared;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.Requests;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class RequestService : IRequestService
{
    private const string QueueName = "send-order";
    private readonly BaseUrlConfiguration _baseUrlConfig;
    private readonly IAppLogger<Order> _appLogger;

    public RequestService(BaseUrlConfiguration baseUrlConfig, IAppLogger<Order> appLogger)
    {
        _baseUrlConfig = baseUrlConfig;
        _appLogger = appLogger;
    }

    public async Task SendOrderRequestAsync(Order order)
    {
        if (order == null)
        {
            return;
        }

        var orderRequest = new OrderRequest
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            OrderDate = order.OrderDate,
            ShipToAddress = new Entities.Requests.Address(
                order.ShipToAddress.Street,
                order.ShipToAddress.City,
                order.ShipToAddress.State,
                order.ShipToAddress.Country,
                order.ShipToAddress.ZipCode),
            OrderItems = order.OrderItems.Select(o => new Entities.Requests.OrderItem
            {
                CatalogItemId = o.ItemOrdered.CatalogItemId,
                ProductName = o.ItemOrdered.ProductName,
                UnitPrice = o.UnitPrice,
                Units = o.Units
            }).ToList(),
            TotalPrice = order.Total()
        };

        
        var client = new ServiceBusClient(_baseUrlConfig.ReserveOrderItemFunctionBaseUrl);
        var sender = client.CreateSender(QueueName);
        var message = new ServiceBusMessage(JsonConvert.SerializeObject(orderRequest));
        await sender.SendMessageAsync(message);

        _appLogger.LogInformation($"Order request with orderId={order.Id} was sent to service bus queue {QueueName}");
    }
}
