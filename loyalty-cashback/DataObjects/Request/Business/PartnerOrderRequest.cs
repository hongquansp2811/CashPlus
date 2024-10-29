using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class PartnerOrderRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? customer_id { get; set; }
        public string? order_date { get; set; }
        public string? order_code { get; set; }
        public decimal? total_amount { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? description { get; set; }
        public int? status { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public List<PartnerOrderDetailRequest>? list_item { get; set; }
    }
}
