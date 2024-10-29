using System;

namespace LOYALTY.DataObjects.Request
{
    public class BankAccountRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public int? bank_id { get; set; }
        public string? bank_owner { get; set; }
        public string? bank_no { get; set; }
        public int? status { get; set; }
    }
}
