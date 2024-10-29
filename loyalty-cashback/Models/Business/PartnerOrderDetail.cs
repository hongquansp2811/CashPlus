using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class PartnerOrderDetail : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? partner_order_id { get; set; }
        public Guid? product_id { get; set; }
        public decimal? quantity { get; set; }
        public decimal? amount { get; set; }
        public decimal? total_amount { get; set; }

    }
}
