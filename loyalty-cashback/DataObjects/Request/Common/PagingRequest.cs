using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class PagingRequest
    {
        public int page_no { get; set; }
        public int page_size { get; set; }
    }
}
