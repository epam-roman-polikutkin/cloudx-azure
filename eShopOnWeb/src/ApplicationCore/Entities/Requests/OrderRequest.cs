using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.Requests;

[Serializable]
public class OrderRequest
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("buyerId")]
    public string BuyerId { get; set; }

    [JsonProperty("orderDate")]
    public DateTimeOffset OrderDate { get; set; }

    [JsonProperty("shipToAddress")]
    public Address ShipToAddress { get; set; }

    [JsonProperty("orderItems")]
    public List<OrderItem> OrderItems { get; set; }

    [JsonProperty("totalPrice")]
    public decimal TotalPrice { get; set; }
}
