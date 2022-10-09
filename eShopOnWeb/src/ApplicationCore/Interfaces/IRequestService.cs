﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;
public interface IRequestService
{
    Task SendOrderRequestAsync(Order order);
    Task SendDeliveryProcessOrderRequestAsync(Order order);
}
