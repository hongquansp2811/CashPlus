using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class RecallPointOrder : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? trans_no { get; set; }
        public Guid? user_id { get; set; }
        public decimal? point_value { get; set; }
    }
}
