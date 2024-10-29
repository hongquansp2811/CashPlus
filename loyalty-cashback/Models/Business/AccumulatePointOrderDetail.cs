using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class AccumulatePointOrderDetail
    {
        public Guid? id { get; set; }
        public Guid? accumulate_point_order_id { get; set; }
        public string? name { get; set; }
        public decimal? discount_rate { get; set; }
        public string? allocation_name { get; set; }
        public decimal? point_value { get; set; }
        public string? description { get; set; }
    }
}
