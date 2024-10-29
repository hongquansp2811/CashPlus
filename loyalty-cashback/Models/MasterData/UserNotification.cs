using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class UserNotification
    {
        public Guid? notification_id { get; set; }
        public Guid? user_id { get; set; }
        public DateTime? date_read { get; set; }
    }
}
