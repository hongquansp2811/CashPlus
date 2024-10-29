using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class ImageInfo
    {
        public string resourceType { get; set; }
        public CurrentFolder currentFolder { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
