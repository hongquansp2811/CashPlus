using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class NotificationRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? user_id { get; set; }
        public Guid? type_id { get; set; }
        public string? title { get; set; }
        public string? avatar { get; set; }
        public string? content { get; set; }
        public string? description { get; set; }

    }
}
