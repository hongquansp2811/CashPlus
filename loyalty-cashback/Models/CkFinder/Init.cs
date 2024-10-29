using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Init
    {
        public bool enabled { get; set; }
        public string s { get; set; }
        public string c { get; set; }
        public List<string> thumbs { get; set; }
        public Images images { get; set; }
        public int uploadMaxSize { get; set; }
        public bool uploadCheckImages { get; set; }
        public List<ResourceTypes> resourceTypes { get; set; }
    }
}
