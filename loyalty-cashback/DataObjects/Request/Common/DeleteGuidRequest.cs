using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class DeleteGuidRequest
    {
        public Guid? id { get; set; }
        public int? status_id { get; set; }
        public string? trans_code { get; set; }
        public string? secret_key { get; set; }
        public string? new_password { get; set; }
        public string? reason_fail { get; set; }
        public List<Guid>? ids { get; set; }
    }
}
