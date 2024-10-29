using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class AffiliateConfigDetail : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? affiliate_config_id { get; set; }
        public int? levels { get; set; }
        public decimal? discount_rate { get; set; }
    }
}
