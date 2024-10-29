using System;

namespace LOYALTY.DataObjects.Request
{
    public class AccumulatePointConfigDetailRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? accumulate_point_config_id { get; set; }
        public string? name { get; set; }
        public decimal? discount_rate { get; set; }
        public int? allocation_id { get; set; }
        public string? allocation_name { get; set; }
        public string? description { get; set; }
    }
}
