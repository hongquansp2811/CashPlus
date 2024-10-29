using System;

namespace LOYALTY.DataObjects.Request
{
    public class ConfigHistoryRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? config_type { get; set; }
        public string? data_logging { get; set; }
        public string? description { get; set; }
    }
}
