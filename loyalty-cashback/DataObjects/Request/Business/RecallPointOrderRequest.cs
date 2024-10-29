using System;

namespace LOYALTY.DataObjects.Request
{
    public class RecallPointOrderRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? user_id { get; set; }
        public decimal? point_value { get; set; }
    }
}
