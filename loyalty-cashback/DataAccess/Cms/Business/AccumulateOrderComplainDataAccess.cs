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
    public class AccumulatePointOrderComplainDataAccess : IAccumulatePointOrderComplain
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AccumulatePointOrderComplainDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AccumulatePointOrderComplainRequest request)
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
            var lstData = (from p in _context.AccumulatePointOrderComplains
                           join b in _context.AccumulatePointOrders on p.accumulate_order_id equals b.id into bs
                           from b in bs.DefaultIfEmpty()
                           join c in _context.Customers on b.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on b.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               accumulate_order_id = p.accumulate_order_id,
                               customer_phone = c.phone,
                               trans_no = b.trans_no,
                               user_created = p.user_created,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringFull(p.date_created),
                               content = p.content,
                               status = p.status,
                               image_links = p.image_links,
                               video_links = p.video_links,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()) || x.customer_phone.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.from_date != null && request.from_date.Length > 0)
            {
                lstData = lstData.Where(x => x.date_created_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null && request.to_date.Length > 0)
            {
                lstData = lstData.Where(x => x.date_created_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.user_created != null)
            {
                lstData = lstData.Where(x => x.user_created == request.user_created);
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

        public APIResponse create(AccumulatePointOrderComplainRequest request, string username)
        {
            if (request.content == null)
            {
                return new APIResponse("ERROR_CONTENT_MISSING");
            }

            if (request.accumulate_order_id == null)
            {
                return new APIResponse("ERROR_ACCUMULATE_POINT_ORDER_ID_MISSING");
            }


            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Tạo cửa hàng
                var data = new AccumulatePointOrderComplain();
                data.id = Guid.NewGuid();
                data.content = request.content;
                data.accumulate_order_id = request.accumulate_order_id;
                data.image_links = request.image_links;
                data.video_links = request.video_links;
                data.status = 19;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.AccumulatePointOrderComplains.Add(data);
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

        public APIResponse updateStatus(AccumulatePointOrderComplainRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.AccumulatePointOrderComplains.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                data.status = request.status;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }
    }
}
