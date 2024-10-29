using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class EstimateApprovePoint : MasterCommonModel
    {
        public Guid? id { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public string? description { get; set; }
    }
}
