using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class Settings : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public decimal? point_value { get; set; }
        public decimal? point_exchange { get; set; }
        public string? policy { get; set; }
        public string? add_point_policy { get; set; }
        public decimal? sms_point_config { get; set; }
        public decimal? change_point_estimate { get; set; }
        public decimal? approve_change_point_min { get; set; }
        public Boolean? is_review { get; set; }
        public string? consumption_point_description { get; set; }
        public string? affiliate_point_description { get; set; }
        public string? app_customer_banner_image { get; set; }
        public string? app_customer_spinner_image { get; set; }
        public string? app_customer_wallet_image { get; set; }
        public string? app_customer_qr_image { get; set; }
        public string? app_customer_affiliate_image { get; set; }
        public string? app_customer_gift_image { get; set; }
        public string? app_partner_spinner { get; set; }
        public decimal? cash_condition_value { get; set; }
        public int? total_allow_cash { get; set; }
        public string? partner_consumption_point_description { get; set; }
        public string? partner_affiliate_point_description { get; set; }
        public string? app_partner_wallet_image { get; set; }
        public string? app_partner_qr_image { get; set; }
        public string? app_partner_employee_image { get; set; }
        public string? app_partner_transaction_image { get; set; }
        public int? sys_receive_bank_id { get; set; }
        public string? sys_receive_bank_no { get; set; }
        public string? sys_receive_bank_name { get; set; }
        public string? sys_receive_bank_owner { get; set; }
        public string? eligible { get; set; } //đủ điều kiện
        public string? unconditional { get; set; } //Không đủ điều kiện
        public string? name_Company { get; set; }  //Tên cty
        public string? address_Company { get; set; }  //Tên cty
        public string? DKKD { get; set; } //Số đăng ký kinh doanh
        public string?  phone_Company { get; set; } //Số điện thoại
        public string? Email_Company { get; set; } //Email công ty
        public decimal? point_use { get; set; } // Điểm tiêu dùng
        public decimal? point_save { get; set; } // Điểm tích lũy 
        public int? send_time { get; set; } // Số lần gửi
        public decimal? collection_fee { get; set; } // Phí thu hộ
        public decimal? expense_fee { get; set; } //Phí chi hộ
        public decimal? payment_limit { get; set; } //Hạn mức thanh toán tối thiểu
        public decimal? amount_limit { get; set; } //Hạn mức chi

    }
}
