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
    public class AccumulatePointOrderRatingDataAccess : IAccumulatePointOrderRating
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AccumulatePointOrderRatingDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

      
        public APIResponse getList(AccumulatePointOrderRatingRequest request)
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
            var lstData = (from p in _context.AccumulatePointOrderRatings
                           join ord in _context.AccumulatePointOrders on p.accumulate_point_order_id equals ord.id
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = ord.trans_no,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringFull(p.date_created),
                               partner_id = s.id,
                               partner_code = s.code,
                               partner_name = s.name,
                               customer_phone = c.phone,
                               customer_name = c.full_name,
                               content = p.content,
                               rating = p.rating,
                               rating_name = p.rating_name
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.partner_id == request.partner_id);
            }

            if (request.rating != null)
            {
                lstData = lstData.Where(x => x.rating == request.rating);
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
