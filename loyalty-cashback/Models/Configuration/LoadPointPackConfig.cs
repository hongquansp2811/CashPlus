using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class LoadPointPackConfig : MasterCommonModel
    {
        public Guid? id { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? value_exchange { get; set; }
        public string? description { get; set; }
    }
}
