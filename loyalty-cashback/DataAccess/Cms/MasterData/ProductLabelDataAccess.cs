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
    public class ProductLabelDataAccess : IProductLabel
    {
        private readonly LOYALTYContext _context;
        public ProductLabelDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(ProductLabelRequest request)
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

            var lstData = (from p in _context.ProductLabels
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name,
                               description = p.description,
                               orders = p.orders
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
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList.OrderBy(p => p.orders) };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = (from p in _context.ProductLabels
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            code = p.code,
                            name = p.name,
                            description = p.description,
                            avatar = p.avatar,
                            orders = p.orders
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(ProductLabelRequest request, string username)
        {

            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataCode = _context.ProductLabels.Where(x => x.code == request.code).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            var dataEx = _context.ProductLabels.Where(l => l.orders == request.orders).FirstOrDefault();
            if(dataEx != null)
            {
                return new APIResponse("Thứ tự hiển thị đã tồn tại");
            }

            try
            {
                var data = new ProductLabel();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.avatar = request.avatar;
                data.orders = request.orders;
                data.status = 1;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.ProductLabels.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(ProductLabelRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.ProductLabels.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataCode = _context.ProductLabels.Where(x => x.code == request.code && x.id != request.id).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if(request.orders != data.orders)
            {
                var dataEx = _context.ProductLabels.Where(l => l.orders == request.orders).FirstOrDefault();
                if (dataEx != null)
                {
                    return new APIResponse("Thứ tự hiển thị đã tồn tại");
                }
            }

            try
            {
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.avatar = request.avatar;
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
            var data = _context.ProductLabels.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.ProductLabels.Remove(data);
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
