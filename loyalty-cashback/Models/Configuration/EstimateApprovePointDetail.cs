using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class EstimateApprovePointDetail : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? estimate_approve_point { get; set; }
        public int? config_type_id { get; set; }
        public decimal? estimate_point { get; set; }
    }
}
