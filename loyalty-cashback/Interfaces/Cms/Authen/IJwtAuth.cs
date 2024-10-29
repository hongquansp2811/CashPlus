using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Interfaces
{
    public interface IJwtAuth
    {
        string Authentication(string username,Guid id, string password, string usertype, string allpermissions);
        string BranchAuthentication(string username, string password, string usertype, Guid customer_id);
        string BKAuthentication(string apiKey, string secretkey);
    }
}
