using System;

namespace LOYALTY.OrderByHQ.Models
{
    public class invoice
    {
        public Guid id { get; set; }
        public double? total_gross_amount { get; set; }
        public double? total_net_amount { get; set; }
        public double? subtotal_gross_amount { get; set; }
        public double? subtotal_net_amount { get; set; }
        public double? cashbackt_amount { get; set; }
        public double? cashbackt_precent { get; set; }
        public int? status { get; set; }
        public Guid partner_table_id { get; set; }
        public Guid? customer_id { get; set; }
        public string? name { get; set; }
        public string? phone { get; set; }
        public string? note { get; set; }
        public DateTime? date_created { get; set; }
        public string? created_by { get; set; }
        public string invoice_code { get; set; }
    }
}
