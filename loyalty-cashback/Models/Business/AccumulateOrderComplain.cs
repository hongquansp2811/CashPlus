using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class AccumulatePointOrderComplain : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? accumulate_order_id { get; set; }
        public string? content { get; set; }
        public string? image_links { get; set; }
        public string? video_links { get; set; }
        public int? status { get; set; }
    }
}
