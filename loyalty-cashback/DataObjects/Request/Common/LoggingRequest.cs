using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class LoggingRequest
    {
        public Guid? id { get; set; }
        public string user_type { get; set; }
        public string application { get; set; }
        public string functions { get; set; }
        public string actions { get; set; }
        public string IP { get; set; }
        public string content { get; set; }
        public string result_logging { get; set; }
        public Boolean is_login { get; set; }
        public Boolean is_call_api { get; set; }
        public string user_created { get; set; }
        public string? api_name { get; set; }
        public DateTime? date_created { get; set; }
    }
}
