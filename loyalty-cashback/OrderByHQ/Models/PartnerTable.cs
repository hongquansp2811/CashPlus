using System;

namespace LOYALTY.OrderByHQ.Models
{
    public class PartnerTable
    {
        public Guid id { get; set; }
        public Guid partner_id { get; set; }
        public int? partner_branch_id { get; set; }
        public string name { get; set; }
        public int floor {  get; set; }
        public int? capacity { get; set; }
        public string? area { get; set; }
        public int? status { get; set; }

    }
}
