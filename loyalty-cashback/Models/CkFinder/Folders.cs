using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Folders
    {
        public string name { get; set; }
        public bool hasChildren { get; set; }
        public int acl { get; set; }
    }
}
