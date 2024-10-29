using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class AccumulatePointOrderRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? order_id { get; set; }
        public string? trans_no { get; set; }
        public Guid? customer_id { get; set; }
        public Guid? partner_id { get; set; }
        public decimal? bill_amount { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? point_avaiable { get; set; }
        public decimal? point_waiting { get; set; }
        public decimal? point_partner { get; set; }
        public int? status { get; set; }
        public string? approve_user { get; set; }
        public DateTime? approve_date { get; set; }
        public string? description { get; set; }
        public string? reason_fail { get; set; }
        public string? files { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public string? user_type { get; set; }
        public string? search { get; set; }
        public string? payment_type { get; set; }
        public string? return_type { get; set; }
        public string? session_id { get; set; }
        public string? payment_gate_response { get; set; }
        public string? payment_gate_response_date { get; set; }
        public List<int?>? list_status { get; set; }
        public List<string>? list_payment_type { get; set; }
    }
}
