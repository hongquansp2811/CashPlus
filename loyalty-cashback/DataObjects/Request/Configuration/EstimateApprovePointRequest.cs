using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class EstimateApprovePointRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public string? description { get; set; }
        public List<EstimateApprovePointDetailRequest>? list_items { get; set; }
    }
}
