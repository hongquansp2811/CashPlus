using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class UserGroupPermissionRequest : PagingRequest
    {
        public Guid? user_group_id { get; set; }
        public List<UserGroupPermission>? list_permission { get; set; }
    }
}
