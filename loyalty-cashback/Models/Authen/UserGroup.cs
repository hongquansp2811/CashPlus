using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class UserGroup : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int? status { get; set; }
        public string? description { get; set; }
    }
}
