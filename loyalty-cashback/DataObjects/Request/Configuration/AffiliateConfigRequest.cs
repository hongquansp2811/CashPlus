using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class AffiliateConfigRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public Guid? service_type_id { get; set; }
        public decimal? discount_rate { get; set; }
        public Boolean? active { get; set; }
        public string? description { get; set; }
        public int? date_return { get; set; }
        public string? hours_return { get; set; }
        public List<AffiliateConfigDetailRequest>? list_items { get; set; }
    }
}
