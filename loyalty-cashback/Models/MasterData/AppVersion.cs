using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class AppVersion
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? version_name { get; set; }
        public int? build { get; set; }
        public string? platform { get; set; }
        public Boolean? is_active { get; set; }
        public Boolean? is_require_update { get; set; }
        public DateTime? apply_date { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
