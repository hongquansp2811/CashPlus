
using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class LoadPointPackConfig : PagingRequest
    {
        public Guid? id { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? value_exchange { get; set; }
        public string? description { get; set; }
    }
}
