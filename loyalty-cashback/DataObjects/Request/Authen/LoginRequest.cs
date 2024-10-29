using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class LoginRequest 
    {
        public string? name { get; set; }
        public string? full_name { get; set; }
        public string? phone_number { get; set; }
        public string? email { get; set; }
        public string? otp_code { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? share_code { get; set; }
        public Boolean? is_android { get; set; }
        public string? device_id { get; set; }
    }
}
