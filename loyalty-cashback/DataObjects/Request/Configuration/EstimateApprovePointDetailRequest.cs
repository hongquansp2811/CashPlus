using System;

namespace LOYALTY.DataObjects.Request
{
    public class EstimateApprovePointDetailRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? estimate_approve_point { get; set; }
        public int? config_type_id { get; set; }
        public decimal? estimate_point { get; set; }
    }
}
