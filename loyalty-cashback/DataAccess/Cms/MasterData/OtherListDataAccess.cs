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
    public class OtherListDataAccess : IOtherList
    {
        private readonly LOYALTYContext _context;
        public OtherListDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(OtherListRequest request)
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

            var lstOtherList = (from p in _context.OtherLists
                             join f in _context.OtherListTypes on p.type equals f.id into fs
                             from f in fs.DefaultIfEmpty()
                             orderby p.date_created descending
                             select new
                             {
                                 id = p.id,
                                 code = p.code,
                                 name = p.name,
                                 description = p.description,
                                 status = p.status,
                                 orders = p.orders,
                                 type = p.type,
                                 type_name = f == null ? "" : f.name,
                                 user_created = p.user_created,
                                 user_updated = p.user_updated,
                                 date_created = p.date_created,
                                 date_updated = p.date_updated
                             });
            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstOtherList = lstOtherList.Where(x => x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            // Đếm số lượng
            int countElements = lstOtherList.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstOtherList.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(int id)
        {
            var action = (from p in _context.OtherLists
                          join f in _context.OtherListTypes on p.type equals f.id into fs
                          from f in fs.DefaultIfEmpty()
                          where p.id == id
                          select new
                          {
                              id = p.id,
                              code = p.code,
                              name = p.name,
                              description = p.description,
                              orders = p.orders,
                              status = p.status,
                              type = p.type,
                              type_name = f == null ? "" : f.name,
                              user_created = p.user_created,
                              user_updated = p.user_updated,
                              date_created = p.date_created,
                              date_updated = p.date_updated
                          }).FirstOrDefault();
            if (action == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(action);
        }

        public APIResponse create(OtherListRequest request, string username)
        {
            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataSame = _context.OtherLists.Where(x => x.code == request.code).FirstOrDefault();

            if (dataSame != null)
            {
                return new APIResponse("ERROR_CODE_EXIST");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            if (request.type == null)
            {
                return new APIResponse("ERROR_TYPE_ID_MISSING");
            }

            try
            {
                var data = new OtherList();
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.status = 1;
                data.type = request.type;
                data.orders = request.orders;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.OtherLists.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(OtherListRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }

            if (request.type == null)
            {
                return new APIResponse("ERROR_FUNC_ID_MISSING");
            }
            var data = _context.OtherLists.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                if (request.code != null && request.code.Length > 0)
                {
                    data.code = request.code;
                    var dataSame = _context.OtherLists.Where(x => x.code == request.code && x.id != request.id).FirstOrDefault();

                    if (dataSame != null)
                    {
                        return new APIResponse("ERROR_CODE_EXIST");
                    }

                }

                if (request.name != null && request.name.Length > 0)
                {
                    data.name = request.name;
                }

                if (request.type != null)
                {
                    data.type = request.type;
                }

                data.description = request.description;
                data.orders = request.orders;
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

        public APIResponse delete(DeleteRequest req)
        {
            var data = _context.OtherLists.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.OtherLists.Remove(data);
                _context.SaveChanges();

                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_DELETE_FAIL");
            }

            return new APIResponse(200);
        }
    }
}
