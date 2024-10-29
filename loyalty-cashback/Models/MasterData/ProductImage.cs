using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class ProductImage
    {
        public Guid? id { get; set; }
        public Guid? product_id { get; set; }
        public string? name { get; set; }
        public string? links { get; set; }
    }
}
