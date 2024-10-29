using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class CustomerRankConfig : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? customer_rank_id { get; set; }
        public string? customer_rank_name { get; set; }
        public decimal? condition_upgrade { get; set; }
    }
}
