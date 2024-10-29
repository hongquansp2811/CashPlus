using System;

namespace LOYALTY.DataObjects.Request
{
    public class AppSuggestSearchRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? name { get; set; }
        public Guid? service_type_id { get; set; }
    }
}
