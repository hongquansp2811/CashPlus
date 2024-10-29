using System;
using System.Collections.Generic;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class CustomerRankConfigRequest : PagingRequest
    {
        public List<CustomerRankConfig>? list_items { get; set; }
    }
}
