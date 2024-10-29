using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class PartnerDocument : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? partner_id { get; set; }
        public string? file_name { get; set; }
        public string? links { get; set; }

    }
}
