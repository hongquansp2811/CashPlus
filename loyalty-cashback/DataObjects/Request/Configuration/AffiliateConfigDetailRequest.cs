using System;

namespace LOYALTY.DataObjects.Request
{
    public class AffiliateConfigDetailRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? affiliate_config_id { get; set; }
        public int? levels { get; set; }
        public decimal? discount_rate { get; set; }
    }
}
