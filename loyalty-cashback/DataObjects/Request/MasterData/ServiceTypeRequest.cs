using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class ServiceTypeRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? icons { get; set; }
        public int? status { get; set; }
        public int? orders { get; set; }
        public decimal? discount_rate { get; set; }

    }
}
