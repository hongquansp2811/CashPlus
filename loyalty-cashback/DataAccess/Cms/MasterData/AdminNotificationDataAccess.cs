using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using Syncfusion.DocIO;

namespace LOYALTY.DataAccess
{
    public class AdminNotificationDataAccess : IAdminNotification
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AdminNotificationDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }
        public APIResponse update(NotificationRequest request, Guid? user_id)
       {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Notifications.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                UserNotification us = new UserNotification();
                us.notification_id = data.id;
                us.user_id = user_id;
                us.date_read = DateTime.Now;
                _context.UserNotifications.Add(us);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req)
        {

            var data = _context.Notifications.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.Notifications.Remove(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public APIResponse getListNoti(NotificationRequest request, Guid? user_id)
        {
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            var lstData = (from p in _context.Notifications
                           join u in _context.UserNotifications.Where(x => x.user_id == user_id) on p.id equals u.notification_id into us
                           from u in us.DefaultIfEmpty()
                           join t in _context.NotificationTypes on p.type_id equals t.id into ts
                           from t in ts.DefaultIfEmpty()
                           where  p.user_id == null || p.user_id == user_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               type_id = p.type_id,
                               tit = p.title,
                               avatar = t.icons,
                               description = p.description,
                               is_read = u != null ? true : false,
                               date_created = p.date_created != null ? _commonFunction.convertDateToStringFull(p.date_created) : "",
                               system_type = p.system_type,
                               reference_id = p.reference_id,
                               content = p.content,
                               user_id = p.user_id,
                           });

            if (request.type_id != null)
            {
                lstData = lstData.Where(x => x.type_id == request.type_id || x.type_id == Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361"));
            }
            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList.OrderByDescending(p => p.date_created) };
            return new APIResponse(dataResult);
        }
    }
}
