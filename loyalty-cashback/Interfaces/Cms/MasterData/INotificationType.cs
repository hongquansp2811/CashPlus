using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface INotificationType
    {
        public APIResponse getList(NotificationTypeRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(NotificationTypeRequest request, string username);
        public APIResponse update(NotificationTypeRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
