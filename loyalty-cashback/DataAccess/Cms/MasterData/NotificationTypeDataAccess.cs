
using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class NotificationTypeDataAccess : INotificationType
    {
        private readonly LOYALTYContext _context;
        public NotificationTypeDataAccess(LOYALTYContext context) 
        {
            this._context = context;
        }

        public APIResponse getList(NotificationTypeRequest request)
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
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstNotificationType = (from p in _context.NotificationTypes
                                   orderby p.date_created descending
                                   select new
                                   {
                                       id = p.id,
                                       code = p.code,
                                       name = p.name,
                                       icons = p.icons
                                   });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstNotificationType = lstNotificationType.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            // Đếm số lượng
            int countElements = lstNotificationType.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstNotificationType.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = _context.NotificationTypes.Where(x => x.id == id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(NotificationTypeRequest request, string username)
        {
            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            var dataCode = _context.NotificationTypes.Where(x => x.code == request.code).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            try
            {
                var data = new NotificationType();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.icons = request.icons;
                data.orders = request.orders;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.NotificationTypes.Add(data);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }
         
            return new APIResponse(200);
        }

        public APIResponse update(NotificationTypeRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.NotificationTypes.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.name != null && request.name.Length > 0)
            {
                data.name = request.name;
            }

            try
            {
                data.name = request.name;
                data.icons = request.icons;
                data.orders = request.orders;
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req)
        {
            var data = _context.NotificationTypes.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.NotificationTypes.Remove(data);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse(400);
            }
           
            return new APIResponse(200);
        }
    }
}
