using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Banner : MasterCommonModel
    {
        public int? id { get; set; }
        public string? title { get; set; }
        public string? title_url { get; set; }
        public string? content { get; set; }
        public string? image_link { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int? status { get; set; }
        public decimal? per_click { get; set; }
        public int? orders { get; set; }
    }
}
