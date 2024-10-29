using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class SMSHistoryReq
    {
        public Guid? id { get; set; }
        public string? SMSID { get; set; }
        public string? CodeResult { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
