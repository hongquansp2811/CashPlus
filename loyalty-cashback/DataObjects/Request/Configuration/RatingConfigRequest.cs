using System;
using System.Collections.Generic;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class RatingConfigRequest : PagingRequest
    {
        public List<RatingConfig>? list_items { get; set; }
    }
}
