using System;

namespace LOYALTY.DataObjects.Request
{
    public class AccumulatePointOrderComplainRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? accumulate_order_id { get; set; }
        public string? content { get; set; }
        public string? image_links { get; set; }
        public string? video_links { get; set; }
        public int? status { get; set; }
        public string? trans_no { get; set; }
        public string? user_created { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
    }
}
