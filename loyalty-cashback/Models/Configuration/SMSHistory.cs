using System;

namespace LOYALTY.Models
{
    public class SMSHistory : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? SMSID { get; set; }
        public string? CodeResult { get; set; }
        public string? ErrorMessage { get; set; }
        public string? phone { get; set; }
        public string? message { get; set; }
    }
}
