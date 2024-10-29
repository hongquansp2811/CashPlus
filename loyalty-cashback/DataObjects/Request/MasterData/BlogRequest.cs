using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class BlogRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? blog_category_id { get; set; }
        public string? title { get; set; }
        public string? title_url { get; set; }
        public string? description { get; set; }
        public string? content { get; set; }
        public string? avatar { get; set; }
        public string? files { get; set; }
        public string? date_blog { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public int? views { get; set; }
    }
}
