using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class CustomerBankAccount : MasterCommonModel
    {
        public Guid? id { get; set; }
        public int? type_id { get; set; }
        public Guid? user_id { get; set; }
        public int? bank_id { get; set; }
        public string? bank_owner { get; set; }
        public string? bank_no { get; set; }
        public string? bank_branch { get; set; }
        public Boolean? is_default { get; set; }
        public DateTime? date_expire { get; set; }
    }
}
