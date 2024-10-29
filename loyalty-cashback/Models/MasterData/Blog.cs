using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class Blog : MasterCommonModel
    {
        public Guid? id { get; set; }
        public Guid? blog_category_id { get; set; }
        public string? title { get; set; }
        public string? title_url { get; set; }
        public string? description { get; set; }
        public string? content { get; set; }
        public string? avatar { get; set; }
        public string? files { get; set; }
        public DateTime? date_blog { get; set; }
        public int? views { get; set; }
    }
}
