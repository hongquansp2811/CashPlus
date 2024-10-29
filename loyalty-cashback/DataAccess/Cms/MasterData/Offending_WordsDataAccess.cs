using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using Syncfusion.DocIO;

namespace LOYALTY.DataAccess
{
    public class Offending_WordsDataAccess : IOffending_Words
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public Offending_WordsDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }
        public APIResponse getList(Offending_WordsReq request)
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

            var lst = _context.offending_Words.OrderByDescending(p => p.date_created).ToList();
            // Nếu tồn tại Where theo tên

            if(request.text != null)
            {
                lst = lst.Where(l => l.text.Contains(request.text)).ToList();
            }

            // Đếm số lượng
            int countElements = lst.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lst.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = _context.offending_Words.Where(l => l.id == id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(Offending_WordsReq request, string username)
        {
            if (request.text == null)
            {
                return new APIResponse("ERROR_TEXT_MISSING");
            }

            try
            {
                var data = new Offending_Words();
                data.id = Guid.NewGuid();
                data.text = request.text;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.offending_Words.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(Offending_WordsReq request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.offending_Words.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            try
            {
                data.text = request.text;
                data.date_updated = DateTime.Now;
                data.user_updated = username;
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
            var data = _context.offending_Words.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.offending_Words.Remove(data);
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
