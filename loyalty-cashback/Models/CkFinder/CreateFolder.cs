using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class CreateFolder
    {
        public string resourceType { get; set; }
        public CurrentFolder currentFolder { get; set; }
        public string newFolder { get; set; }
        public int created { get; set; }
    }
}
