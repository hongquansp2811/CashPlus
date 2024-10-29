using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class SecretKeyRequest
    {
        public string? otp_code { get; set; }
        public string? new_secret_key { get; set; }
        public string? old_secret_key { get; set; }
    }
}
