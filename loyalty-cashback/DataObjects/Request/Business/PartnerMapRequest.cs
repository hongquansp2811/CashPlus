using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class PartnerMapRequest : PagingRequest
    {
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public decimal? radius { get; set; }
    }
}
