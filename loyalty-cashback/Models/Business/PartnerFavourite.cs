using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class PartnerFavourite
    {
        public Guid? customer_id { get; set; }
        public Guid? partner_id { get; set; }
    }
}
