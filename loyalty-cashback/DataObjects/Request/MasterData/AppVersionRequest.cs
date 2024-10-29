using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class AppVersionRequest : PagingRequest
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? version_name { get; set; }
        public int? build { get; set; }
        public string? platform { get; set; }
        public Boolean? is_active { get; set; }
        public Boolean? is_require_update { get; set; }
        public string? apply_date { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
    }
}
