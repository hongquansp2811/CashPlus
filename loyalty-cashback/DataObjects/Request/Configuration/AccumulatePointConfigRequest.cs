using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class AccumulatePointConfigRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public Guid? contract_id { get; set; }
        public Guid? service_type_id { get; set; }
        public Guid? partner_id { get; set; }
        public decimal? discount_rate { get; set; }
        public Boolean? active { get; set; }
        public string? description { get; set; }
        public string? search { get; set; }
        public int? status { get; set; }
        public List<AccumulatePointConfigDetailRequest>? list_items { get; set; }
    }
}
