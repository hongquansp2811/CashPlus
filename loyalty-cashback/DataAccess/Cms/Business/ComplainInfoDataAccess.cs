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
    public class ComplainInfoDataAccess : IComplainInfo
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public ComplainInfoDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(ComplainInfoRequest request)
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
            var lstData = (from p in _context.ComplainInfos
                           select new
                           {
                               id = p.id,
                               name = p.name
                           });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstData = lstData.Where(x => x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
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
            var data = (from p in _context.ComplainInfos
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            name = p.name
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(ComplainInfoRequest request, string username)
        {
            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Tạo cửa hàng
                var data = new ComplainInfo();
                data.id = Guid.NewGuid();
                data.name = request.name;
                _context.ComplainInfos.Add(data);
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

        public APIResponse update(ComplainInfoRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.ComplainInfos.Where(x => x.id == request.id).FirstOrDefault();
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
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest request)
        {
            var data = _context.ComplainInfos.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.ComplainInfos.Remove(data);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_DELETE_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }
    }
}
