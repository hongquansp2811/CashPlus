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
    public class StaticPageDataAccess : IStaticPage
    {
        private readonly LOYALTYContext _context;
        public StaticPageDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(StaticPageRequest request)
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

            var lstData = (from p in _context.StaticPages
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name,
                               icon = p.icon
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
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = (from p in _context.StaticPages
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            code = p.code,
                            name = p.name,
                            content = p.content,
                            icon = p.icon
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getDetailByCode(string code)
        {
            var data = (from p in _context.StaticPages
                        where p.code == code
                        select new
                        {
                            code = p.code,
                            content = p.content
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(StaticPageRequest request, string username)
        {

            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataCode = _context.StaticPages.Where(x => x.code == request.code).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            try
            {
                var data = new StaticPage();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.content = request.content;
                data.icon = request.icon;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.StaticPages.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(StaticPageRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.StaticPages.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataCode = _context.StaticPages.Where(x => x.code == request.code && x.id != request.id).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            try
            {
                data.icon = request.icon;
                data.code = request.code;
                data.name = request.name;
                data.content = request.content;
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
            var data = _context.StaticPages.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.StaticPages.Remove(data);
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
