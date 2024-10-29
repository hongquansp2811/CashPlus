using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class AppSuggestSearchDataAccess : IAppSuggestSearch
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppSuggestSearchDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AppSuggestSearchRequest request)
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
            var lstData = (from p in _context.AppSuggestSearchs
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               name = p.name,
                               service_type_id = p.service_type_id,
                               service_type_name = sv.name
                           });

            // Nếu tồn tại Where theo tên
            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
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
            var data = (from p in _context.AppSuggestSearchs
                        join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                        from sv in svs.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            name = p.name,
                            service_type_id = p.service_type_id,
                            service_type_name = sv.name
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(AppSuggestSearchRequest request, string username)
        {

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                var data = new AppSuggestSearch();
                data.id = Guid.NewGuid();
                data.service_type_id = request.service_type_id;
                data.name = request.name;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.AppSuggestSearchs.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_ADD_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }

        public APIResponse update(AppSuggestSearchRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.AppSuggestSearchs.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            try
            {
                data.name = request.name;
                data.service_type_id = request.service_type_id;
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
            var data = _context.AppSuggestSearchs.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.AppSuggestSearchs.Remove(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(400);
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }
    }
}
