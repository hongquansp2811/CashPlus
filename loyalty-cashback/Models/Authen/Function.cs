using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Function : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int? status { get; set; }
        public string? description { get; set; }
        public string? url { get; set; }
        public Boolean? is_default { get; set; }
    }
}
