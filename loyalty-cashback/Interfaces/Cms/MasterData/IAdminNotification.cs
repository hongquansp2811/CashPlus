using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAdminNotification
    {
        public APIResponse update(NotificationRequest request, Guid? user_id);
        public APIResponse delete(DeleteGuidRequest req);
        public APIResponse getListNoti(NotificationRequest request, Guid? user_id);
    }
}
