using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class Offending_WordsReq : PagingRequest
    {
        public Guid? id { get; set; }
        public string? text { get; set; }
    }
}
