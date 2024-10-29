using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class GetFolders
    {
        public string resourceType { get; set; }
        public CurrentFolder currentFolder { get; set; }
        public List<Folders> folders { get; set; }
    }
}
