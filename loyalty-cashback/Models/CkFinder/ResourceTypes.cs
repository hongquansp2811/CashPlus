using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class ResourceTypes
    {
        public string name { get; set; }
        public string url { get; set; }
        public string allowedExtensions { get; set; }
        public string deniedExtensions { get; set; }
        public string hash { get; set; }
        public bool hasChildren { get; set; }
        public int acl { get; set; }
        public int maxSize { get; set; }
    }
}
