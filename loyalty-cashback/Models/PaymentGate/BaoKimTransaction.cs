using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class BaoKimTransaction
    {
        public Guid? id { get; set; }
        public string? payment_type { get; set; }
        public string? transaction_no { get; set; }
        public string? bao_kim_transaction_id { get; set; }
        public DateTime? transaction_date { get; set; }
        public string? bank_receive_name { get; set; }
        public string? bank_receive_account { get; set; }
        public string? bank_receive_owner { get; set; }
        public decimal? amount { get; set; }
        public int? trans_status { get; set; }
        public string? trans_log { get; set; }
        public string? transaction_description { get; set; }
        public Guid? accumulate_point_order_id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? customer_id { get; set; }
        public decimal? amount_balance { get; set; }    
    }
}
