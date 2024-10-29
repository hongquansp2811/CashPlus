using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Response
{
    public class DataListResponse
    {
        public int total_elements { get; set; }
        public int total_page { get; set; }
        public int page_no { get; set; }
        public int page_size { get; set; }
        public object data { get; set; }
        public decimal? data_count { get; set; }
        public decimal? values { get; set; }
        public decimal? data_quantity { get; set; }
    }
}
