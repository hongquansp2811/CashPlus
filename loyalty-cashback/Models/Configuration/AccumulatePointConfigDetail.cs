using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class AccumulatePointConfigDetail : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? accumulate_point_config_id { get; set; }
        public string? name { get; set; }
        public decimal? discount_rate { get; set; }
        public int? allocation_id { get; set; }
        public string? allocation_name { get; set; }
        public string? description { get; set; }
    }
}
