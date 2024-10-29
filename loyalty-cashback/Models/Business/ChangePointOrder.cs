using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class ChangePointOrder : MasterCommonModel
    {
        public Guid? id { get; set; }
        public int? trans_type_id { get; set; }
        public string? trans_no { get; set; }
        public Guid? user_id { get; set; }
        public Guid? customer_bank_account_id { get; set; }
        public int? user_type_id { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? exchange_rate { get; set; }
        public decimal? value_exchange { get; set; }
        public int? status { get; set; }
        public DateTime? approve_date { get; set; }
        public string? reason_fail { get; set; }
        public string? files { get; set; }

    }
}
