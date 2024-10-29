using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class PartnerOrder : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? customer_id { get; set; }
        public DateTime? order_date { get; set; }
        public string? order_code { get; set; }
        public decimal? total_amount { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? description { get; set; }
        public int? status { get; set; }

    }
}
