using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.DataObjects.Request
{
    public class UserGroupRequest : PagingRequest
    {
        public Guid? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int? status { get; set; }
        public string? description { get; set; }
        public List<UserGroupPermission>? userGroupPermissions { get; set; }
    }
}
