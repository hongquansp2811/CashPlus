﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class ProvinceRequest : PagingRequest
    {
        public int? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public int? types { get; set; }
        public int? parent_id { get; set; }
        public int? province_id { get; set; }
        public int? district_id { get; set; }
        public int? orders { get; set; }
    }
}