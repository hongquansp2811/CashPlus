using System;

namespace LOYALTY.OrderByHQ.Models
{
    public class Order
    {
        public Guid id { get; set; }
        public decimal? total_gross_amount { get; set; }
        public decimal? total_net_amount { get; set; }
        public decimal? subtotal_gross_amount { get; set; }
        public decimal? subtotal_net_amount { get; set; }
        public decimal? extra_charge_amount { get; set; }
        public string? note { get; set; }
        public DateTime? created_date { get; set; }
        public string? created_by { get; set; }
        public Guid partner_table_id { get; set; }
        public Guid invoice_id { get; set; }
        public int? status { get; set; }
        public Guid? customer_id { get; set; }
        public string? name { get; set; }
        public string? phone { get; set; }
        public int? order_type { get; set; }
    }
}
