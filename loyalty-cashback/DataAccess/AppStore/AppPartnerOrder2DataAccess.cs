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
    public class AppPartnerOrder2DataAccess : IPartnerOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppPartnerOrder2DataAccess(LOYALTYContext context, ICommonFunction commonFunction)
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
                           join s in _context.Customers on p.customer_id equals s.id
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           select new
                           {
                               id = p.id,
                               order_code = p.order_code,
                               order_date = p.order_date,
                               customer_name = s.full_name,
                               customer_phone = s.phone,
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
                            customer_name = s.full_name,
                            customer_phone = s.phone,
                            status = p.status,
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
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse confirm(Guid id, string username)
        {
            var data = _context.PartnerOrders.Where(x => x.id == id).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_ORDER_ID_INCORRECT");
            }

            if (data.status != 3)
            {
                return new APIResponse("ERROR_ORDER_NOT_CONFIRM");
            }

            var transaction = _context.Database.BeginTransaction();

            try
            {
                data.status = 5;

                data.user_updated = username;
                data.date_updated = DateTime.Now;

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
    }
}
