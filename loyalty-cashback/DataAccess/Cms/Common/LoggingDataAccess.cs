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
    public class LoggingDataAccess : ILogging
    {
        private readonly LOYALTYContext _context;
        public LoggingDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getListLogIn(FilterLoggingRequest request)
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
            var lstOtherListType = (from p in _context.Loggings
                                    where p.is_login == true
                                    orderby p.date_created descending
                                    select new { 
                                        user_created = p.user_created,
                                        application = p.application,
                                        actions = p.actions,
                                        IP = p.IP,
                                        is_login = p.is_login,
                                        date_created = p.date_created,
                                        result_logging = p.result_logging
                                    });

            // Nếu tồn tại Where theo tên
            if (request.search != null && request.search.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.user_created.Contains(request.search) || x.actions.Contains(request.search));
            }

            if (request.applications != null && request.applications.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.application.Contains(request.applications));
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
        public APIResponse getListAction(FilterLoggingRequest request)
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
            var lstOtherListType = (from p in _context.Loggings
                                    where p.is_login == false && p.is_call_api == true
                                    orderby p.date_created descending
                                    select new
                                    {
                                        user_created = p.user_created,
                                        functions = p.functions,
                                        actions = p.actions,
                                        IP = p.IP,
                                        is_login = p.is_login,
                                        is_call_api = p.is_call_api,
                                        date_created = p.date_created,
                                        result_logging = p.result_logging
                                    });

            // Nếu tồn tại Where theo tên
            if (request.search != null && request.search.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.user_created.Contains(request.search) || x.actions.Contains(request.search));
            }

            if (request.functions != null && request.functions.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.functions.Contains(request.functions));
            }

            if (request.results != null && request.results.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.result_logging.Contains(request.results));
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

        public APIResponse getListCallApi(FilterLoggingRequest request)
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
            var lstOtherListType = (from p in _context.Loggings
                                    where p.is_call_api == true
                                    orderby p.date_created descending
                                    select new
                                    {
                                        user_created = p.user_created,
                                        application = p.application,
                                        actions = p.actions,
                                        content = p.content,
                                        IP = p.IP,
                                        api_name = p.api_name,
                                        is_login = p.is_login,
                                        date_created = p.date_created,
                                        result_logging = p.result_logging
                                    });

            // Nếu tồn tại Where theo tên
            if (request.search != null && request.search.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.user_created.Contains(request.search) || x.actions.Contains(request.search));
            }

            if (request.applications != null && request.applications.Length > 0)
            {
                lstOtherListType = lstOtherListType.Where(x => x.application.Contains(request.applications));
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

    }
}
