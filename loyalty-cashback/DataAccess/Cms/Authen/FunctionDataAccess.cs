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
    public class FunctionDataAccess : IFunction
    {
        private readonly LOYALTYContext _context;
        public FunctionDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(FunctionRequest request)
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
            var lstFunction = (from p in _context.Functions
                               orderby p.date_created descending
                               select new
                               {
                                   id = p.id,
                                   code = p.code,
                                   name = p.name,
                                   status = p.status,
                                   description = p.description,
                                   url = p.url,
                                   is_default = p.is_default
                               });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstFunction = lstFunction.Where(x => x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) 
                || x.description.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            // Đếm số lượng
            int countElements = lstFunction.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstFunction.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var Function = (from p in _context.Functions
                            where p.id == id
                            select new
                            {
                                id = p.id,
                                code = p.code,
                                name = p.name,
                                status = p.status,
                                description = p.description,
                                url = p.url,
                                is_default = p.is_default
                            }).FirstOrDefault();
            if (Function == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(Function);
        }

        public APIResponse create(Function request, string username)
        {
            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.url == null)
            {
                return new APIResponse("ERROR_URL_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            try
            {
                var data = new Function();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.url = request.url;
                data.status = 1;
                data.is_default = request.is_default;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.Functions.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(Function request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Functions.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                if (request.code != null && request.code.Length > 0)
                {
                    data.code = request.code;
                }

                if (request.name != null && request.name.Length > 0)
                {
                    data.name = request.name;
                }

                data.url = request.url;
                data.description = request.description;
                data.is_default = request.is_default;
                if (request.status != null)
                {
                    data.status = request.status;
                }
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
            var data = _context.Functions.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.Functions.Remove(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public APIResponse getFunctionTree()
        {
            var lstFunction = (from p in _context.Functions
                               select new
                               {
                                   id = p.id,
                                   code = p.code,
                                   name = p.name,
                                   status = p.status,
                                   description = p.description,
                                   parent_id = "",
                                   url = p.url,
                                   list_actions = _context.Actions.Where(x => x.function_id == p.id).ToList(),
                                   is_parent = false
                               }).ToList();

            return new APIResponse(new
            {
                data = lstFunction
            });
        }

        public APIResponse getListFunctionPermission()
        {
            var listFunctions = (from p in _context.Actions
                                 join f in _context.Functions on p.function_id equals f.id
                                 select new
                                 {
                                     action_id = p.id,
                                     action_name = p.name,
                                     function_id = f.id,
                                     function_name = f.name
                                 }).ToList();
            return new APIResponse(listFunctions);
        }

    }
}
