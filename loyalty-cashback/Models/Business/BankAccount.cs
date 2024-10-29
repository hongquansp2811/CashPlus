using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class BankAccount : MasterCommonModel
    {
        public Guid? id { get; set; }
        public int? bank_id { get; set; }
        public string? bank_owner { get; set; }
        public string? bank_no { get; set; }
        public int? status { get; set; }
    }
}
