using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LOYALTY.Models
{
    public class OtherListType: MasterCommonModel
    {
        public int? id { get; set; }

        public string? code { get; set; }

        public string? name { get; set; }

        public int? status { get; set; }

        public string? description { get; set; }

        public int? orders { get; set; }
    }
}
