using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class BankRequest : PagingRequest
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? avatar { get; set; }
        public string? background { get; set; }
        public string? description { get; set; }
        public Boolean? active { get; set; }
    }
}
