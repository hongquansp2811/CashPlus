using System;

namespace DataObjects.Response
{
    public class PartnerResponse
    {
        public Guid? partner_id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? avatar { get; set; }
        public string? description { get; set; }
        public int? status { get; set; }
    }
}
