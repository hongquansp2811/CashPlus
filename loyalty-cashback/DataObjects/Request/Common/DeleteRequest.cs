using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class DeleteRequest
    {
        public int? id { get; set; }
        public int? status_id { get; set; }
        public string? trans_code { get; set; }
        public List<int>? ids { get; set; }
    }
}
