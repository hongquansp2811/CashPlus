using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class ConfigHistory : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? config_type { get; set; }
        public string? data_logging { get; set; }
        public string? description { get; set; }
        public int? type { get; set; } //1: Cấu hình chung ; 2 Cấu hình thông báo    
    }
}
