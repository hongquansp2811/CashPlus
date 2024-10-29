using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class PartnerContract : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? contract_name { get; set; }
        public string? contract_no { get; set; }
        public DateTime? sign_date { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public int? contract_type_id { get; set; }
        public Guid? service_type_id { get; set; }
        public Guid? partner_id { get; set; }
        public decimal? discount_rate { get; set; }
        public string? contact_name { get; set; }
        public string? phone { get; set; }
        public string? tax_code { get; set; }
        public string? files { get; set; }
        public decimal? warning_estimate { get; set; }
        public decimal? warning_estimate_hour { get; set; }
        public int? status { get; set; }
        public Guid? support_person_id { get; set; }
        public string? support_person_phone { get; set; }
        public string? description { get; set; }
        public Boolean? is_delete { get; set; }
        public Boolean? is_GENERAL { get; set; }

    }
}
