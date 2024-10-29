using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class UserPermissionRequest : PagingRequest
    {
        public Guid? user_id { get; set; }
        public List<UserPermission>? list_permission { get; set; }
    }
}
