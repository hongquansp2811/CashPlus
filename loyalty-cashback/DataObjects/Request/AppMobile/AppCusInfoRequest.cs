using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class AppCusInfoRequest
    {
        public int? id { get; set; }
        public string? username { get; set; }
        public string? full_name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? avatar { get; set; }
        public string? old_otp_code { get; set; }
        public string? otp_code { get; set; }
        public string? password { get; set; }
        public string? birth_date { get; set; }
        public string? device_id { get; set; }
        public Boolean? send_Notification { get; set; } //Gửi thông báo
        public Boolean? send_Popup { get; set; } //Gửi Popup
        public Boolean? SMS_addPointSave { get; set; } //Gửi tin nhắn điểm tích lũy
        public Boolean? SMS_addPointUse { get; set; } //Gửi tin nhắn điểm tiêu dùng
    }
}
