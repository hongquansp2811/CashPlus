using System;

namespace LOYALTY.DataObjects.Request
{
    public class AddPointOrderRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? trans_no { get; set; }
        public Guid? partner_id { get; set; }
        public decimal? bill_amount { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? point_avaiable { get; set; }
        public decimal? point_waiting { get; set; }
        public int? status { get; set; }
        public DateTime? approve_date { get; set; }
        public string? description { get; set; }
        public string? reason_fail { get; set; }
        public string? files { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public string? search { get; set; }
    }
}
