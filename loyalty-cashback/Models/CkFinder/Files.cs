using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Files
    {
        public string name { get; set; }
        public string url { get; set; }
        public string date { get; set; }
        public int size { get; set; }
        public int type { get; set; }
        public double? width { get; set; }
        public double? height { get; set; }
        public string extension { get; set; }
        public string path { get; set; }
        public DateTime? dateCreate { get; set; }
        public string alt { get; set; }
        public string note { get; set; }
        public string userName { get; set; }
        public int year { get; set; }
        public int month { get; set; }
    }
}
