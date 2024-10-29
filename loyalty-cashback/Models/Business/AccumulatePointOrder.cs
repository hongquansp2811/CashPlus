using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class AccumulatePointOrder : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? trans_no { get; set; }
        public Guid? customer_id { get; set; }
        public Guid? partner_id { get; set; }
        public decimal? bill_amount { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? point_avaiable { get; set; }
        public decimal? point_waiting { get; set; }
        public decimal? point_partner { get; set; }
        public decimal? discount_rate { get; set; }
        public decimal? point_customer { get; set; }
        public decimal? point_system { get; set; }
        public int? status { get; set; }
        public string? approve_user { get; set; }
        public DateTime? approve_date { get; set; }
        public string? description { get; set; }
        public string? reason_fail { get; set; }
        public string? files { get; set; }
        public string? address { get; set; }
        public string? payment_type { get; set; }
        public string? session_id { get; set; }
        public string? payment_gate_response { get; set; }
        public string? return_type { get; set; }
        public DateTime? payment_gate_response_date { get; set; }
        public decimal? amount_customer { get; set; }
        public decimal? amount_partner { get; set; }
        public decimal? amount_system { get; set; }

    }
}
