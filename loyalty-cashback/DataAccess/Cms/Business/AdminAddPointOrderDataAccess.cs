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
    public class AdminAddOrderDataAccess : IAdminAddPointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AdminAddOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AddPointOrderRequest request)
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

            var lstData = (from p in _context.AddPointOrders
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join cf in _context.CustomerFakeBanks on p.partner_id equals cf.user_id into cfs
                           from cf in cfs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                               date_created = p.date_created,
                               partner_id = p.partner_id,
                               partner_code = s.code,
                               partner_name = s.name,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_exchange,
                               bank_name = cf != null ? cf.bank_name : "",
                               bank_no = cf != null ? cf.bank_account : "",
                               status_name = "Hoàn thành"
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.partner_id == request.partner_id);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created <= _commonFunction.convertStringSortToDate(request.to_date));
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
    }
}
