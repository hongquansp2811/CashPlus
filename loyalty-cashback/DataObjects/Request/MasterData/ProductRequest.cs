using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class ProductRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? product_group_id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? customer_id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public decimal? price { get; set; }
        public int? status { get; set; }
        public string? description { get; set; }
        public string? detail_info { get; set; }
        public string? avatar { get; set; }
        public string? reason_fail { get; set; }
        public List<ProductImage>? list_images { get; set; }
        public List<int>? list_status_not_in { get; set; }
        //ThienDev
        public int? number { get; set; }
        public Boolean? status_change { get; set; }
    }
}
