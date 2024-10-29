using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class AccumulatePointConfig : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public Guid? contract_id { get; set; }
        public Guid? service_type_id { get; set; }
        public Guid? partner_id { get; set; }
        public decimal? discount_rate { get; set; }
        public int? status { get; set; }
        public Boolean? active { get; set; }
        public string? description { get; set; }
    }
}
