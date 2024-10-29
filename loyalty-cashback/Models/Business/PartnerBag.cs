using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class PartnerBag : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? product_id { get; set; }
        public Guid? customer_id { get; set; }
        public decimal? quantity { get; set; }
        public decimal? amount { get; set; }
        public decimal? total_amount { get; set; }

    }
}
