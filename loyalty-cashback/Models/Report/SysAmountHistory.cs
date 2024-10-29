using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class SysAmountHistory
    {
        public int? id { get; set; }
        public string? type { get; set; }
        public int? years { get; set; }
        public int? quarter { get; set; }
        public int? month { get; set; }
        public int? day { get; set; }
        public string? trans_type { get; set; }
        public decimal? amount { get; set; }
        public Boolean? is_balance { get; set; }
        public DateTime? trans_date { get; set; }
    }
}
