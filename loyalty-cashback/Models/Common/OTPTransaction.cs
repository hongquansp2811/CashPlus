using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class OTPTransaction
    {
        public string? otp_code { get; set; }
        public DateTime? date_created { get; set; }
        public int? object_id { get; set; }
        public string? object_type { get; set; }
        public string? object_name { get; set; }
        public DateTime? date_limit { get; set; }
        public string phone_number { get; set; }
    }
}
