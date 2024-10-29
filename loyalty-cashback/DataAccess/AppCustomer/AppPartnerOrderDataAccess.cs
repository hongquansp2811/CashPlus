using System;
using System.Linq;
using System.Collections.Generic;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using DataObjects.Response;
using System.Data;
using System.Threading.Tasks;
using LOYALTY.Services;
using Dapper;

namespace LOYALTY.DataAccess
{
    public class AppPartnerOrderResponse
    {
        public Guid? id { get; set; }
        public string? order_code { get; set; }
        public DateTime? order_date_origin { get; set; }
        public string? order_date { get; set; }
        public string? partner_name { get; set; }
        public string? partner_address { get; set; }
        public int? status { get; set; }
        public string? status_name { get; set; }
        public decimal? total_amount { get; set; }
        public decimal? total_quantity { get; set; }
        public List<AppPartnerOrderDetailResponse>? list_items { get; set; }
    }

    public class AppPartnerOrderDetailResponse
    {
        public string? product_name { get; set; }
        public string? product_avatar { get; set; }
        public decimal? price { get; set; }
        public decimal? quantity { get; set; }
        public decimal? total_amount { get; set; }
    }

    public class AppPartnerOrderDataAccess : IAppPartnerOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly IDapper _dapper;
        public AppPartnerOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction, IDapper dapper)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _dapper = dapper;
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
                           join s in _context.Partners on p.partner_id equals s.id
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           orderby p.order_date descending
                           select new
                           {
                               id = p.id,
                               order_code = p.order_code,
                               order_date = p.order_date,
                               discount_rate = s.customer_discount_rate,
                               partner_name = s.name,
                               partner_address = s.address,
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
                        join s in _context.Partners on p.partner_id equals s.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            order_code = p.order_code,
                            order_date = p.order_date,
                            partner_name = s.name,
                            status = p.status,
                            discount_rate = s.customer_discount_rate,
                            list_items = (from d in _context.PartnerOrderDetails
                                          join pr in _context.Products on d.product_id equals pr.id
                                          where d.partner_order_id == p.id
                                          select new
                                          {
                                              product_name = pr.name,
                                              quantity = d.quantity,
                                              total_amount = d.total_amount,
                                              discount_rate = s.customer_discount_rate,
                                              product_avatar = pr.avatar
                                          }).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(PartnerOrderRequest request, string username)
        {
            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.list_item == null || request.list_item.Count == 0)
            {
                return new APIResponse("ERROR_LIST_ITEM_MISSING");
            }

            var transaction = _context.Database.BeginTransaction();

            try
            {
                Guid orderId = Guid.NewGuid();
                decimal total_amount = 0;

                for (int i = 0; i < request.list_item.Count; i++)
                {
                    // Thêm bản ghi detail
                    var newDetail = new PartnerOrderDetail();
                    newDetail.id = Guid.NewGuid();
                    newDetail.partner_order_id = orderId;
                    newDetail.product_id = request.list_item[i].product_id;
                    newDetail.amount = request.list_item[i].amount != null ? request.list_item[i].amount : 0;
                    newDetail.quantity = request.list_item[i].quantity != null ? request.list_item[i].quantity : 1;
                    newDetail.total_amount = newDetail.amount * newDetail.quantity;
                    total_amount += (decimal)newDetail.total_amount;
                    newDetail.user_created = username;
                    newDetail.date_created = DateTime.Now;

                    _context.PartnerOrderDetails.Add(newDetail);

                    // Xóa giỏ hàng
                    var itemBag = _context.PartnerBags.Where(x => x.customer_id == request.customer_id && x.partner_id == request.partner_id && x.product_id == request.list_item[i].product_id).FirstOrDefault();

                    if (itemBag != null)
                    {
                        _context.PartnerBags.Remove(itemBag);
                    }
                }

                _context.SaveChanges();

                var maxCodeObject = _context.PartnerOrders.Where(x => x.order_code != null && x.order_code.Contains("DH")).OrderByDescending(x => x.order_code).FirstOrDefault();
                string codeOrder = "";
                if (maxCodeObject == null)
                {
                    codeOrder = "DH0000000000001";
                }
                else
                {
                    string maxCode = maxCodeObject.order_code;
                    maxCode = maxCode.Substring(2);
                    int orders = int.Parse(maxCode);
                    orders = orders + 1;
                    string orderString = orders.ToString();
                    char pad = '0';
                    int number = 13;
                    codeOrder = "DH" + orderString.PadLeft(number, pad);
                }

                var data = new PartnerOrder();
                data.id = orderId;
                data.customer_id = request.customer_id;
                data.partner_id = request.partner_id;
                data.order_code = codeOrder;
                data.order_date = DateTime.Now;
                data.phone = request.phone;
                data.email = request.email;
                data.description = request.description;
                data.total_amount = total_amount;
                data.status = 3;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.PartnerOrders.Add(data);
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

        public APIResponse getListProductGroup(CategoryRequest request)
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

            var lstData = (from p in _context.Products
                           join s in _context.Partners on p.partner_id equals s.id
                           where s.service_type_id == request.service_type_id
                           group p by new
                           {
                               p.product_group_id,
                           } into e
                           select new ProductGroupResponse
                           {
                               product_group_id = e.Key.product_group_id,
                           }).ToList();

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            foreach (var item in dataList)
            {
                var itemPG = _context.ProductGroups.Where(e=>e.id == item.product_group_id).FirstOrDefault();
                if(itemPG != null)
                {
                    item.code = itemPG.code;    
                    item.name = itemPG.name;    
                    item.avatar = itemPG.avatar;    
                    item.description = itemPG.description;    
                    item.status = itemPG.status;    
                }
            }
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };

            
            return new APIResponse(dataResult);
        }

        public APIResponse getListPartner(CategoryRequest request)
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

            var lstData = (from p in _context.Products
                           join s in _context.Partners on p.partner_id equals s.id
                           where p.product_group_id == request.product_group_id
                           group p by new
                           {
                               p.partner_id,
                           } into e
                           select new PartnerResponse
                           {
                               partner_id = e.Key.partner_id,
                           }).ToList();

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            foreach (var item in dataList)
            {
                var itemPG = _context.Partners.Where(e => e.id == item.partner_id).FirstOrDefault();
                if (itemPG != null)
                {
                    item.partner_id = itemPG.id;
                    item.code = itemPG.code;
                    item.name = itemPG.name;
                    item.avatar = itemPG.avatar;
                    item.description = itemPG.description;
                    item.status = itemPG.status;
                }
            }
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };


            return new APIResponse(dataResult);
        }

        public APIResponse getListPartnerTest(PartnerMapRequest request)
        {
            var pram = new DynamicParameters();
            pram.Add("lat", request.latitude, DbType.Decimal);
            pram.Add("long", request.longitude, DbType.Decimal);
            pram.Add("radius", request.radius, DbType.Decimal);
            //var dataList = Task.FromResult(_dapper.GetAll<Partner>($"Select * from Partner", null, commandType: CommandType.Text));
            var dataList = Task.FromResult(_dapper.GetAll<Partner>("GetPartnersInRadius", pram, commandType: CommandType.StoredProcedure)).Result;
            // Đếm số lượng
            int countElements = dataList.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };


            return new APIResponse(dataResult);
        }


    }
}
