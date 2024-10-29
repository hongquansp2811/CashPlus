using LOYALTY.Models;
using System;
using System.Collections.Generic;

namespace LOYALTY.DataObjects.Request
{
    public class PartnerFilterRequest
    {
        public Guid? service_type_id { get; set; }
        public Guid? product_group_id { get; set; }
        public Guid? product_label_id { get; set; }
        public int? province_id { get; set; }
        public int? district_id { get; set; }
        public int? ward_id { get; set; }
        public string? search { get; set; }
        public Coordinate north_east { get; set; }
        public Coordinate south_west { get; set; }
        public double latitude { get; set; }
        public double longtitude { get; set; }
        public bool is_update_zoom { get; set; } = false;

    }

    public sealed class Coordinate
    {
        public double latitude { get; set; }
        public double longtitude { get; set; }
    }
}
