using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;
using DocumentFormat.OpenXml.VariantTypes;

namespace LOYALTY.DataAccess
{
    public class ServiceTypeDataAccess : IServiceType
    {
        private readonly LOYALTYContext _context;
        public ServiceTypeDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(ServiceTypeRequest request)
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

            var lstData = (from p in _context.ServiceTypes
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name,
                               orders = p.orders,
                               description = p.description
                           });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstData = lstData.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }


            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList.OrderByDescending(p => p.orders) };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = (from p in _context.ServiceTypes
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            code = p.code,
                            name = p.name,
                            description = p.description,
                            discount_rate = p.discount_rate,
                            orders = p.orders,
                            icons = p.icons
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(ServiceTypeRequest request, string username)
        {

            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataCode = _context.ServiceTypes.Where(x => x.code == request.code).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            var order = _context.ServiceTypes.Where(x => x.orders == request.orders).FirstOrDefault();
            if(order != null)
            {
                return new APIResponse("Thứ tự hiển thị đã được sử dụng");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            try
            {
                var data = new ServiceType();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.discount_rate = request.discount_rate;
                data.icons = request.icons;
                data.orders = request.orders;
                data.status = 1;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.ServiceTypes.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(ServiceTypeRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.ServiceTypes.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataCode = _context.ServiceTypes.Where(x => x.code == request.code && x.id != request.id).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if(data.orders != request.orders)
            {
                var order = _context.ServiceTypes.Where(x => x.orders == request.orders).FirstOrDefault();
                if (order != null)
                {
                    return new APIResponse("Thứ tự hiển thị đã được sử dụng");
                }
            }

            try
            {
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.icons = request.icons;
                data.discount_rate = request.discount_rate;
                data.orders = request.orders;
                data.user_updated = username;
                data.date_updated = DateTime.Now;

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
            var data = _context.ServiceTypes.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            var dataExist = _context.Partners.Where(p => p.service_type_id == data.id).FirstOrDefault();
            if(dataExist != null)
            {
                return new APIResponse("Loại dịch vụ đã có thông tin đối tác");
            }
            try
            {
                _context.ServiceTypes.Remove(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }
    }
}
