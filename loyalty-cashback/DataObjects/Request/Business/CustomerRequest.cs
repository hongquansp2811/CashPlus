using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class CustomerRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? full_name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? birth_date { get; set; }
        public Guid? share_person_id { get; set; }
        public Guid? customer_rank_id { get; set; }
        public int? status { get; set; }
        public decimal? tax_tncn { get; set; }
        public string? avatar { get; set; }
        public List<CustomerBankAccount>? list_bank_accounts { get; set; }
        public Guid? customer_id { get; set; }
        public string? search { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public int? level { get; set; }
        public string? order_by_condition { get; set; }
        public string? order_by_type { get; set; }
    }
}
