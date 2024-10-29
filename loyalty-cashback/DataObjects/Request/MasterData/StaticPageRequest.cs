using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class StaticPageRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? content { get; set; }
        public string? icon { get; set; }
    }
}
