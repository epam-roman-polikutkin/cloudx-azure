using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.Requests;

[Serializable]
public class Address
{
    public Address(string street, string city, string state, string country, string zipcode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipcode;
    }

    [JsonProperty("street")]
    public string Street { get; private set; }

    [JsonProperty("city")]
    public string City { get; private set; }

    [JsonProperty("state")]
    public string State { get; private set; }

    [JsonProperty("country")]
    public string Country { get; private set; }

    [JsonProperty("zipCode")]
    public string ZipCode { get; private set; }
    
}
