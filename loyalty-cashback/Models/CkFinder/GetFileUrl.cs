using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class GetFileUrl
    {
        public string resourceType { get; set; }
        public CurrentFolder currentFolder { get; set; }
        public string url { get; set; }
    }
}
