using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface INotification
    {
        public APIResponse getList(NotificationRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(NotificationRequest request, string username);
        public APIResponse update(NotificationRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
