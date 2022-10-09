using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BlazorShared;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.Requests;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class RequestService : IRequestService
{
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

        using (var client = new HttpClient())
        {
            var strContent = JsonConvert.SerializeObject(orderRequest);
            var content = new StringContent(strContent);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await client.PostAsync(
                $"{_baseUrlConfig.ReserveOrderItemFunctionBaseUrl}?orderId={order.Id}", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            _appLogger.LogInformation(resultContent);
        }
    }
}
