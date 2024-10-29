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
    public class OtherListTypeDataAccess : IOtherListType
    {
        private readonly LOYALTYContext _context;
        public OtherListTypeDataAccess(LOYALTYContext context) 
        {
            this._context = context;
        }

        public APIResponse getList(OtherListTypeRequest request)
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
            var lstOtherListType = _context.OtherListTypes.OrderByDescending(x => x.date_created).ToList();

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.name.Trim().ToLower().Contains(request.name.Trim().ToLower())).ToList();
            }

            // Đếm số lượng
            int countElements = lstOtherListType.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstOtherListType.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(int id)
        {
            var otherListType = _context.OtherListTypes.Where(x => x.id == id).FirstOrDefault();
            if (otherListType == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(otherListType);
        }

        public APIResponse create(OtherListType request, string username)
        {
            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            var dataSame = _context.OtherListTypes.Where(x => x.code == request.code).FirstOrDefault();

            if (dataSame != null)
            {
                return new APIResponse("ERROR_CODE_EXIST");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            try
            {
                var data = new OtherListType();
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.orders = request.orders;
                data.status = 1;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.OtherListTypes.Add(data);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }
         
            return new APIResponse(200);
        }

        public APIResponse update(OtherListType request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var otherListType = _context.OtherListTypes.Where(x => x.id == request.id).FirstOrDefault();
            if (otherListType == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.code != null && request.code.Length > 0)
            {
                otherListType.code = request.code;
                var dataSame = _context.OtherListTypes.Where(x => x.code == request.code && x.id != request.id).FirstOrDefault();

                if (dataSame != null)
                {
                    return new APIResponse("ERROR_CODE_EXIST");
                }

            }

            if (request.name != null && request.name.Length > 0)
            {
                otherListType.name = request.name;
            }

            otherListType.description = request.description;
            otherListType.orders = request.orders;
            otherListType.status = request.status;
            try
            {
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteRequest req)
        {
            var otherListType = _context.OtherListTypes.Where(x => x.id == req.id).FirstOrDefault();
            if (otherListType == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.OtherListTypes.Remove(otherListType);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse(400);
            }
           
            return new APIResponse(200);
        }
    }
}
