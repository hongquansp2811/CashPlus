using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppNotification
    {
        public APIResponse getListType();
        public APIResponse getList(NotificationRequest request, Guid? user_id);
        public APIResponse getDetail(Guid id, Guid user_id);

    }
}
