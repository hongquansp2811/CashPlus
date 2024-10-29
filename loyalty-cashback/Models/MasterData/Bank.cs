using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Bank : MasterCommonModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? avatar { get; set; }
        public string? background { get; set; }
        public string? description { get; set; }
        public string? bank_code { get; set; }
        public Boolean? active { get; set; }
    }

}
