using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Offending_Words : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? text { get; set; }
    }
}
