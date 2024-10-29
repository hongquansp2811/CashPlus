using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class AccumulatePointOrderRating : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? accumulate_point_order_id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? customer_id { get; set; }
        public decimal? rating { get; set; }
        public string? content { get; set; }
        public string? rating_name { get; set; }
    }
}
