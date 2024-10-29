using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class UserRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? username { get; set; }
        public string? full_name { get; set; }
        public string? avatar { get; set; }
        public string? description { get; set; }
        public string? password { get; set; }
        public string? salt { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public Guid? user_group_id { get; set; }
        public int? status { get; set; }
        public Boolean? is_sysadmin { get; set; }
        public Boolean? is_admin { get; set; }
        public Boolean? is_customer { get; set; }
        public Guid? customer_id { get; set; }
        public string? device_id { get; set; }
        public Boolean? is_partner { get; set; }
        public Boolean? is_partner_admin { get; set; }
        public Guid? partner_id { get; set; }
        public string? secret_key { get; set; }
        public string? share_code { get; set; }
        public Guid? share_person_id { get; set; }
        public decimal? total_point { get; set; }
        public decimal? point_waiting { get; set; }
        public decimal? point_affiliate { get; set; }
        public int? user_type_id { get; set; }
        public decimal? point_avaiable { get; set; }
        public Boolean? is_delete { get; set; }
        public List<UserPermission>? userPermissions { get; set; }
        public string? user_type { get; set; }
        public Boolean? is_add_point_permission { get; set; }
        public Boolean? is_change_point_permission { get; set; }
        public Boolean? is_manage_user { get; set; }
        public Boolean? send_Notification { get; set; } //Gửi thông báo
        public Boolean? send_Popup { get; set; } //Gửi Popup
        public Boolean? SMS_addPointSave { get; set; } //Gửi tin nhắn điểm tích lũy
        public Boolean? SMS_addPointUse { get; set; } //Gửi tin nhắn điểm tiêu dùng
    }
}
