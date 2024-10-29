using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Customer : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? full_name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public DateTime? birth_date { get; set; }
        public Guid? share_person_id { get; set; }
        public Guid? customer_rank_id { get; set; }
        public int? status { get; set; }
        public decimal? tax_tncn { get; set; }
        public string? avatar { get; set; }
        public DateTime? time_otp_limit { get; set; }
        public int? count_otp_fail { get; set; }

    }
}
