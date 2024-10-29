using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class AccumulatePointOrderAffiliate
    {
        public Guid? id { get; set; }
        public Guid? accumulate_point_order_id { get; set; }
        public string? username { get; set; }
        public int? levels { get; set; }
        public decimal? discount_rate { get; set; }
        public decimal? point_value { get; set; }
        public DateTime? date_created { get; set; }
    }
}
