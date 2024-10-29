using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class ActionRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int? status { get; set; }
        public string? description { get; set; }
        public string? url { get; set; }
        public Boolean? is_default { get; set; }
        public Guid? function_id { get; set; }
    }
}
