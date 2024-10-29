using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class AppNotificationDataAccess : IAppNotification
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppNotificationDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getListType()
        {
            var lstData = (from p in _context.NotificationTypes
                           orderby p.orders ascending
                           select new
                           {
                               id = p.id,
                               name = p.name,
                               avatar = p.icons
                           }).ToList();

            return new APIResponse(lstData);
        }

        public APIResponse getDetail(Guid id, Guid user_id)
        {
            var data = (from p in _context.Notifications
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            tit = p.title,
                            avatar = p.avatar,
                            content = p.content
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                var dataNoti = _context.UserNotifications.Where(x => x.user_id == user_id && x.notification_id == id).FirstOrDefault();

                if (dataNoti == null)
                {
                    var newDataNoti = new UserNotification();
                    newDataNoti.user_id = user_id;
                    newDataNoti.notification_id = id;
                    newDataNoti.date_read = DateTime.Now;

                    _context.UserNotifications.Add(newDataNoti);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
            return new APIResponse(data);
        }

        public APIResponse getList(NotificationRequest request, Guid? user_id)
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
                           where p.user_id == null || p.user_id == user_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               type_id = p.type_id,
                               tit = p.title,
                               avatar = t.icons,
                               description = p.description,
                               is_read = u != null ? true : false,
                               system_type = p.system_type,
                               date_created = p.date_created != null ? _commonFunction.convertDateToStringFull(p.date_created) : "",
                               ref_id = p.reference_id,
                               user_id = p.user_id,
                           });

            if (request.type_id != null)
            {
                lstData = lstData.Where(x => x.type_id == request.type_id);
            }
            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }
    }
}
