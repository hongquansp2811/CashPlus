using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.Controllers;

namespace LOYALTY.DataAccess
{
    public class ActionDataAccess : IAction
    {
        private readonly LOYALTYContext _context;
        public ActionDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(ActionRequest request)
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

            var lstAction = (from p in _context.Actions
                             join f in _context.Functions on p.function_id equals f.id into fs
                             from f in fs.DefaultIfEmpty()
                             orderby p.date_created descending
                             select new
                             {
                                 id = p.id,
                                 code = p.code,
                                 name = p.name,
                                 description = p.description,
                                 is_default = p.is_default,
                                 status = p.status,
                                 function_id = p.function_id,
                                 function_name = f == null ? "" : f.name,
                                 url = p.url,
                                 user_created = p.user_created,
                                 user_updated = p.user_updated,
                                 date_created = p.date_created,
                                 date_updated = p.date_updated,
                                 action_type = p.action_type
                             });
            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstAction = lstAction.Where(x => x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) 
                || x.description.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            if (request.function_id != null)
            {
                lstAction = lstAction.Where(x => x.function_id == request.function_id);
            }

            // Đếm số lượng
            int countElements = lstAction.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstAction.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var action = (from p in _context.Actions
                          join f in _context.Functions on p.function_id equals f.id into fs
                          from f in fs.DefaultIfEmpty()
                          where p.id == id
                          select new
                          {
                              id = p.id,
                              code = p.code,
                              name = p.name,
                              description = p.description,
                              is_default = p.is_default,
                              status = p.status,
                              function_id = p.function_id,
                              function_name = f == null ? "" : f.name,
                              url = p.url,
                              user_created = p.user_created,
                              user_updated = p.user_updated,
                              date_created = p.date_created,
                              date_updated = p.date_updated,
                              action_type = p.action_type
                          }).FirstOrDefault();
            if (action == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(action);
        }

        public APIResponse create(Action1 request, string username)
        {
            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            // var dataCode = _context.Actions.Where(x => x.code == request.code && x.function_id == request.function_id).FirstOrDefault();

            // if (dataCode != null)
            // {
            //     return new APIResponse("ERROR_CODE_EXISTS");
            // }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            if (request.function_id == null)
            {
                return new APIResponse("ERROR_FUNC_ID_MISSING");
            }

            if (request.url == null)
            {
                return new APIResponse("ERROR_URL_MISSING");
            }

            try
            {
                var data = new Action1();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.status = 1;
                data.function_id = request.function_id;
                data.is_default = request.is_default;
                data.url = request.url;
                data.user_created = username;
                data.user_updated = username;
                data.action_type = request.action_type;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.Actions.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(Action1 request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }

            if (request.function_id == null)
            {
                return new APIResponse("ERROR_FUNC_ID_MISSING");
            }
            var data = _context.Actions.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            // var dataCode = _context.Actions.Where(x => x.code == request.code && x.id != request.id && x.function_id == request.function_id).FirstOrDefault();

            // if (dataCode != null)
            // {
            //     return new APIResponse("ERROR_CODE_EXISTS");
            // }

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

                if (request.function_id != null)
                {
                    data.function_id = request.function_id;
                }
                if(request.action_type != null){
                    data.action_type = request.action_type;
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
            var data = _context.Actions.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            var dataExits = _context.UserGroupPermissions.Where(p => p.action_id == data.id).FirstOrDefault();
            if(dataExits != null){
                return new APIResponse("ERROR_ACTION_EXISTS");
            }
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.Actions.Remove(data);
                _context.SaveChanges();

                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public APIResponse changeStatus(DeleteGuidRequest req)
        {
            var data = _context.Actions.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (req.status_id == null)
            {
                return new APIResponse("ERROR_STATUS_ID_MISSING");
            }

            if (data.status == req.status_id)
            {
                return new APIResponse("ERROR_STATUS_CANNOT_CHANGE");
            }

            try
            {
                data.status = req.status_id;
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
