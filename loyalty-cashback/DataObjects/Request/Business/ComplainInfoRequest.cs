using System;

namespace LOYALTY.DataObjects.Request
{
    public class ComplainInfoRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? name { get; set; }
    }
}
