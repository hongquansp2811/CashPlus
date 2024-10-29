using System;

namespace LOYALTY.DataObjects.Request
{
    public class AccumulatePointOrderRatingRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? accumulate_point_order_id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? customer_id { get; set; }
        public decimal? rating { get; set; }
        public string? content { get; set; }
        public string? rating_name { get; set; }
        // Filter
        public string? trans_no { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }

    }
}
