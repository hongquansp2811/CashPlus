using System;
using System.Collections.Generic;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class ExchangePointPackConfigRequest : PagingRequest
    {
        public List<ExchangePointPackConfig>? list_items { get; set; }
    }
}
