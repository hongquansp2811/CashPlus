using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class ExchangePointPackConfig : MasterCommonModel
    {
        public Guid? id { get; set; }
        public decimal? point_exchange { get; set; }
        public decimal? value_exchange { get; set; }
    }
}
