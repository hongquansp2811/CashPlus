using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class AffiliateConfig : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public Guid? service_type_id { get; set; }
        public Boolean? active { get; set; }
        public string? description { get; set; }
        public int? date_return { get; set; }
        public TimeSpan? hours_return { get; set; }
    }
}
