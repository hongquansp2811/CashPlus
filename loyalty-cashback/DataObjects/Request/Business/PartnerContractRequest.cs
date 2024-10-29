using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class PartnerContractRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? contract_name { get; set; }
        public string? contract_no { get; set; }
        public string? sign_date { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
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
        public Boolean? is_GENERAL { get; set; }
        public AccumulatePointConfigRequest accumulateConfig { get; set; }
    }
}
