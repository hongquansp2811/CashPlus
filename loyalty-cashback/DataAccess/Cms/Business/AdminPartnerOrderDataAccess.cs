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
    public class AdminPartnerOrderDataAccess : IAdminPartnerOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AdminPartnerOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(PartnerOrderRequest request)
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

            var lstData = (from p in _context.PartnerOrders
                           join c in _context.Customers on p.customer_id equals c.id
                           join s in _context.Partners on p.partner_id equals s.id
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.order_date descending
                           select new
                           {
                               id = p.id,
                               order_code = p.order_code,
                               order_date = p.order_date,
                               order_date_2 = _commonFunction.convertDateToStringFull(p.order_date),
                               customer_name = c.full_name,
                               customer_phone = c.phone,
                               partner_id = p.partner_id,
                               partner_name = s.name,
                               status = p.status,
                               status_name = st != null ? st.name : "",
                               total_amount = p.total_amount,
                               total_quantity = _context.PartnerOrderDetails.Where(x => x.partner_order_id == p.id).Sum(x => x.quantity),
                               list_items = (from d in _context.PartnerOrderDetails
                                             join pr in _context.Products on d.product_id equals pr.id
                                             where d.partner_order_id == p.id
                                             select new
                                             {
                                                 product_name = pr.name,
                                                 price = d.amount,
                                                 quantity = d.quantity,
                                                 total_amount = d.total_amount,
                                                 product_avatar = pr.avatar
                                             }).ToList()
                           });

            if (request.order_code != null && request.order_code.Length > 0)
            {
                lstData = lstData.Where(x => x.order_code.Trim().ToLower().Contains(request.order_code.Trim().ToLower())
                || x.customer_phone.Trim().ToLower().Contains(request.order_code.Trim().ToLower()) || x.customer_name.Trim().ToLower().Contains(request.order_code.Trim().ToLower()));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.order_date >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.order_date <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.partner_id == request.partner_id);
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
            var data = (from p in _context.PartnerOrders
                        join s in _context.Customers on p.customer_id equals s.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            order_code = p.order_code,
                            order_date = p.order_date,
                            order_date_2 = _commonFunction.convertDateToStringFull(p.order_date),
                            customer_name = s.full_name,
                            customer_phone = s.phone,
                            customer_email = s.email,
                            status = p.status,
                            total_amount = p.total_amount,
                            list_items = (from d in _context.PartnerOrderDetails
                                          join pr in _context.Products on d.product_id equals pr.id
                                          where d.partner_order_id == p.id
                                          select new
                                          {
                                              product_name = pr.name,
                                              quantity = d.quantity,
                                              total_amount = d.total_amount,
                                              product_avatar = pr.avatar
                                          }).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }
    }
}
