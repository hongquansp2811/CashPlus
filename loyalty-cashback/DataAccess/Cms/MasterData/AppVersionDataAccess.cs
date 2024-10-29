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
    public class AppVersionDataAccess : IAppVersion
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppVersionDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AppVersionRequest request)
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

            var lstAppVersion = (from p in _context.AppVersions
                             orderby p.created_at descending
                             select new
                              {
                                  id = p.id,
                                  name = p.name,
                                  version_name = p.version_name,
                                  build = p.build,
                                  platform = p.platform,
                                  apply_date = _commonFunction.convertDateToStringSort(p.apply_date)
                             });
            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstAppVersion = lstAppVersion.Where(x => x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.version_name.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.platform.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }


            // Đếm số lượng
            int countElements = lstAppVersion.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstAppVersion.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(int id)
        {
            var action = (from p in _context.AppVersions
                          where p.id == id
                          select new
                          {
                              id = p.id,
                              name = p.name,
                              version_name = p.version_name,
                              build = p.build,
                              platform = p.platform,
                              apply_date = _commonFunction.convertDateToStringSort(p.apply_date),
                              created_at = _commonFunction.convertDateToStringSort(p.created_at),
                              updated_at = _commonFunction.convertDateToStringSort(p.updated_at),
                              is_active = p.is_active,
                              is_require_update = p.is_require_update
                          }).FirstOrDefault();
            if (action == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(action);
        }

        public APIResponse create(AppVersionRequest request, string username)
        {
            if (request.version_name == null || request.version_name == "")
            {
                return new APIResponse("ERROR_VERSION_NAME_MISSING");
            }

            if (request.platform == null || request.platform == "")
            {
                return new APIResponse("ERROR_PLATFORM_MISSING");
            }

            var dataCode = _context.AppVersions.Where(x => x.version_name == request.version_name && x.platform == request.platform).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_VERSION_NAME_EXISTS");
            }

            if (request.name == null || request.name == "")
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.build == null)
            {
                return new APIResponse("ERROR_BUILD_MISSING");
            }

            

            if (request.apply_date == null || request.apply_date == "")
            {
                return new APIResponse("ERROR_APPLY_DATE_MISSING");
            }

            try
            {
                var data = new AppVersion();
                data.name = request.name;
                data.version_name = request.version_name;
                data.build = request.build;
                data.platform = request.platform;
                data.is_active = request.is_active != null ? request.is_active : true;
                data.is_require_update = request.is_require_update != null ? request.is_require_update : false;
                data.apply_date = _commonFunction.convertStringSortToDate(request.apply_date);
                data.created_at = DateTime.Now;
                data.updated_at = DateTime.Now;
                _context.AppVersions.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(AppVersionRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            if (request.version_name == null || request.version_name == "")
            {
                return new APIResponse("ERROR_VERSION_NAME_MISSING");
            }

            if (request.platform == null)
            {
                return new APIResponse("ERROR_PLATFORM_MISSING");
            }

            var dataCode = _context.AppVersions.Where(x => x.version_name == request.version_name && x.platform == request.platform && x.id != request.id).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_VERSION_NAME_EXISTS");
            }

            if (request.name == null || request.name == "")
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.build == null)
            {
                return new APIResponse("ERROR_BUILD_MISSING");
            }


            if (request.apply_date == null || request.apply_date == "")
            {
                return new APIResponse("ERROR_APPLY_DATE_MISSING");
            }
            var data = _context.AppVersions.Where(x => x.id == request.id).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_ID_INCORRECT");
            }
            try
            {
                data.name = request.name;
                data.version_name = request.version_name;
                data.build = request.build;
                data.platform = request.platform;
                data.is_active = request.is_active != null ? request.is_active : true;
                data.is_require_update = request.is_require_update != null ? request.is_require_update : false;
                data.apply_date = _commonFunction.convertStringSortToDate(request.apply_date);

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
            var data = _context.AppVersions.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.AppVersions.Remove(data);
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
    }
}
