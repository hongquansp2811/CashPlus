using System;

namespace LOYALTY.DataObjects.Request
{
    public class BonusPointConfigRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public Guid? service_type_id { get; set; }
        public decimal? discount_rate { get; set; }
        public decimal? min_point { get; set; }
        public decimal? max_point { get; set; }
        public string? description { get; set; }
        public Boolean? active { get; set; }
    }
}
