using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class SysAmountSummary
    {
        public int? id { get; set; }
        public int? years { get; set; }
        public int? quarter { get; set; }
        public int? month { get; set; }
        public int? days { get; set; }
        public decimal? open_balance { get; set; }
        public decimal? pull_balance { get; set; }
        public decimal? push_balance { get; set; }
        public decimal? close_balance { get; set; }
    }
}
