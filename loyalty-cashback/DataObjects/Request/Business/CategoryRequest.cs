using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class CategoryRequest : PagingRequest
    {
        public Guid? product_group_id { get; set; }
        public Guid? service_type_id { get; set; }
        
    }
}
