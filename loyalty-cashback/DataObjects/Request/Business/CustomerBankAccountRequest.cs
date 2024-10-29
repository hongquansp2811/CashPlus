using System;

namespace LOYALTY.DataObjects.Request
{
    public class CustomerBankAccountRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public int? type_id { get; set; }
        public Guid? user_id { get; set; }
        public int? bank_id { get; set; }
        public string? bank_owner { get; set; }
        public string? bank_no { get; set; }
        public string? bank_branch { get; set; }
        public string? date_expire { get; set; }
        public string? secret_key { get; set; }
        public Boolean? is_default { get; set; }
    }
}
