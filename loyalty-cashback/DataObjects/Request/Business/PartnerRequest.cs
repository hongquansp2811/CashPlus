using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class PartnerRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? service_type_id { get; set; }
        public Guid? product_group_id { get; set; }
        public int? store_type_id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? start_hour { get; set; }
        public string? end_hour { get; set; }
        public string? working_day { get; set; }
        public string? store_owner { get; set; }
        public string? description { get; set; }
        public string? tax_code { get; set; }
        public Guid? product_label_id { get; set; }
        public decimal? tax_tncn { get; set; }
        public string? username { get; set; }
        public int? province_id { get; set; }
        public string? province_name { get; set; }
        public int? district_id { get; set; }
        public int? ward_id { get; set; }
        public int? status { get; set; }
        public string? address { get; set; }
        public string? latitude { get; set; }
        public string? longtitude { get; set; }
        public string? avatar { get; set; }
        public string? password { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public string? search { get; set; }
        public int? level { get; set; }
        public Boolean? is_delete { get; set; }
        public List<CustomerBankAccount>? list_bank_accounts { get; set; }
        public string? order_by_condition { get; set; }
        public string? order_by_type { get; set; }
        public decimal? discount_rate { get; set; }
        public decimal? rating { get; set; }
        public decimal? total_rating { get; set; }
        public Guid? support_person_id { get; set; }
        public string? support_person_phone { get; set; }
        public string? support_person_email { get; set; }
        // Bổ sung 14/09
        public string? license_no { get; set; }
        public int? license_person_number { get; set; }
        public string? license_owner { get; set; }
        public string? license_birth_date { get; set; }
        public int? license_nation_id { get; set; }
        public string? indetifier_no { get; set; }
        public string? identifier_date { get; set; }
        public string? identifier_at { get; set; }
        public string? identifier_date_expire { get; set; }
        public string? identifier_address { get; set; }
        public int? identifier_province_id { get; set; }
        public Boolean? is_same_address { get; set; }
        public string? now_address { get; set; }
        public int? now_nation_id { get; set; }
        public int? now_province_id { get; set; }
        public string? identifier_front_image { get; set; }
        public string? identifier_back_image { get; set; }
        public List<PartnerDocument>? list_documents { get; set; }
        public string? login_code { get; set; }
        public string? license_image { get; set; }
        public string? license_date { get; set; }
        public int? owner_percent { get; set; }
        public int? identifier_nation_id { get; set; }
        public string? bk_partner_code { get; set; }
        public string? bk_merchant_id { get; set; }
        public string? bk_email { get; set; }
        public string? bk_password { get; set; }
        public int? bk_bank_id { get; set; }
        public string? bk_bank_no { get; set; }
        public string? bk_bank_name { get; set; }
        public string? bk_bank_owner { get; set; }
        public string? API_KEY { get; set; } 
        public string? API_SECRET { get; set; }
        public int Encrypt_status { get; set; }
        public string? RSA_publicKey { get; set; }
        public string? RSA_privateKey { get; set; }
        public string? link_QR { get; set; }

    }
}
