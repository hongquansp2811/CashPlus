using System;

namespace LOYALTY.DataObjects.Request
{
    public class ChangePointOrderRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public int? trans_type_id { get; set; }
        public string? trans_no { get; set; }
        public Guid? user_id { get; set; }
        public Guid? customer_bank_account_id { get; set; }
        public Guid? exchange_pack_point_id { get; set; }
        public int? user_type_id { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? exchange_rate { get; set; }
        public decimal? value_exchange { get; set; }
        public int? status { get; set; }
        public string? approve_date { get; set; }
        public string? reason_fail { get; set; }
        public string? files { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public string? user_type { get; set; }
        public decimal? from_point { get; set; }
        public decimal? to_point { get; set; }
    }
}
