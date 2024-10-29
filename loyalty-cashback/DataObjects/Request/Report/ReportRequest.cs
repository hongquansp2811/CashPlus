using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class ReportRequest : PagingRequest
    {
        public string? search { get; set; }
        public string? period { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public int? status { get; set; }
        public int? user_type_id { get; set; }
        public Guid? partner_id { get; set; }
        public int? years { get; set; }
        public int? quarter { get; set; }
        public List<Guid>? ids { get; set; }
        public List<string>? list_types { get; set; }
        public List<int>? list_status { get; set; }
    }

    public class reportReq : PagingRequest
    {
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public string?  Key { get; set; }
    }
}
