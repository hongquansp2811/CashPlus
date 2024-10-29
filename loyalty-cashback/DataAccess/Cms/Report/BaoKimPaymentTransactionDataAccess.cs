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
    //public class BaoKimPaymentTransactionDataAccess : IBaoKimPaymentTransaction
    //{
    //    private readonly LOYALTYContext _context;
    //    public BaoKimPaymentTransactionDataAccess(LOYALTYContext context)
    //    {
    //        this._context = context;
    //    }

    //    public APIResponse adminPaymentReport(ReportRequest request)
    //    {
    //        // Default page_no, page_size
    //        if (request.page_size < 1)
    //        {
    //            request.page_size = Consts.PAGE_SIZE;
    //        }

    //        if (request.page_no < 1)
    //        {
    //            request.page_no = 1;
    //        }
    //        // Số lượng Skip
    //        int skipElements = (request.page_no - 1) * request.page_size;
    //        //.Take(request.page_size).Skip(skipElements)
    //        // Khai báo mảng ban đầu
    //        var lstDatas = (from p in _context.BaoKimTransactions
    //                        where )

    //        // Nếu tồn tại Where theo tên
    //        //if (request.search != null && request.search.Length > 0)
    //        //{
    //        //    lstDatas = lstDatas.Where(x => x.name.Trim().ToLower().Contains(request.search.Trim().ToLower())).ToList();
    //        //}

    //        // Đếm số lượng
    //        int countElements = lstDatas.Count();

    //        // Số lượng trang
    //        int totalPage = countElements > 0
    //                ? (int)Math.Ceiling(countElements / (double)request.page_size)
    //                : 0;

    //        // Data Sau phân trang
    //        var dataList = lstDatas.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
    //        var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
    //        return new APIResponse(dataResult);
    //    }
    //}
}
