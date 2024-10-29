using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class ProductLabel : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? avatar { get; set; }
        public int? orders { get; set; }
        public int? status { get; set; }
    }
}
