using System;

namespace LOYALTY.DataObjects.Request
{
    public class PartnerBagRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public Guid? partner_id { get; set; }
        public Guid? product_id { get; set; }
        public Guid? customer_id { get; set; }
        public decimal? quantity { get; set; }
        public decimal? amount { get; set; }
        public decimal? total_amount { get; set; }
    }
}
