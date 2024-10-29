using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class FilterLoggingRequest : PagingRequest
    {
        public string? search { get; set; }
        public string? applications { get; set; }
        public string? functions { get; set; }
        public string? results { get; set; }
    }
}

