using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class PasswordRequest
    {
        public string? otp_code { get; set; }
        public string? email { get; set; }
        public string? phone_number { get; set; }
        public string? old_password { get; set; }
        public string? new_password { get; set; }
        public Guid? user_id { get; set; }
    }
}
