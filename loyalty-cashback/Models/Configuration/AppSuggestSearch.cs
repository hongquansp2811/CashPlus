using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class AppSuggestSearch : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? name { get; set; }
        public Guid? service_type_id { get; set; }
    }
}
