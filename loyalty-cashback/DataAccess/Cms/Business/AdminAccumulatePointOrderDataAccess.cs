using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using Org.BouncyCastle.Ocsp;

namespace LOYALTY.DataAccess
{
    public class AdminAccumulateOrderDataAccess : IAdminAccumulatePointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AdminAccumulateOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AccumulatePointOrderRequest request)
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

            var lstData = (from p in _context.AccumulatePointOrders
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                               date_created = p.date_created,
                               customer_phone = c != null ? c.phone : "",
                               partner_code = s.code,
                               partner_name = s.name,
                               bill_amount = p.bill_amount,
                               partner_id = p.partner_id,
                               point_exchange = p.point_exchange,
                               point_customer = p.point_customer,
                               point_system = p.point_system,
                               point_partner = p.point_partner,
                               discount_rate = p.discount_rate,
                               description = p.description,
                               return_type = p.return_type,
                               payment_type = p.payment_type,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.payment_type != null)
            {
                lstData = lstData.Where(x => x.payment_type == request.payment_type);
            }

            if (request.return_type != null)
            {
                lstData = lstData.Where(x => x.return_type == request.return_type);
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

        public APIResponse getDetail(Guid id)
        {
            var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;

            decimal collection_fee = settings != null && settings.collection_fee != null && settings.collection_fee != 0 ? (decimal)settings.collection_fee : 1; //thu hộ
            decimal expense_fee = settings != null && settings.expense_fee != null && settings.expense_fee != 0 ? (decimal)settings.expense_fee : 1; //Chi hộ
            decimal amount_limit = settings != null && settings.amount_limit != null && settings.amount_limit != 0 ? (decimal)settings.amount_limit : 1;


            // var AccumulatePointOrderDetails = _context.AccumulatePointOrderDetails.ToList();

            var data = (from p in _context.AccumulatePointOrders
                        join s in _context.Partners on p.partner_id equals s.id into ss
                        from s in ss.DefaultIfEmpty()
                        join c in _context.Customers on p.customer_id equals c.id into cs
                        from c in cs.DefaultIfEmpty()
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            trans_no = p.trans_no,
                            trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                            date_created = p.date_created,
                            customer_phone = c != null ? c.phone : "",
                            partner_name = s.name,
                            partner_code = s.code,
                            approve_user = p.user_created,
                            bill_amount = p.bill_amount,
                            point_exchange = p.point_exchange,
                            point_customer = p.point_customer,
                            point_system = p.point_system,
                            point_partner = p.point_partner,
                            discount_amount = Math.Round((decimal)p.point_partner * pointExchangeRate),
                            receive_amount = p.bill_amount - Math.Round((decimal)p.point_partner * pointExchangeRate),
                            discount_rate = p.discount_rate,
                            description = p.description,
                            return_type = p.return_type,
                            payment_type = p.payment_type,
                            status = p.status,
                            status_name = st != null ? st.name : "",
                            list_details = _context.AccumulatePointOrderDetails.Where(x => x.accumulate_point_order_id == p.id).ToList(),
                            point_value_count = _context.AccumulatePointOrderDetails.Where(x => x.accumulate_point_order_id == p.id && x.allocation_name != "Người tiêu dùng" && x.point_value != null).Sum(x => x.point_value),
                            //list_bk_trans = _context.BaoKimTransactions.Where(x => x.accumulate_point_order_id == p.id).FirstOrDefault(),
                            list_bk_trans = (from a in _context.BaoKimTransactions
                                             where a.accumulate_point_order_id == p.id
                                             select new
                                             {
                                                 id = a.id,
                                                 amoun2 = a.amount,
                                                //  amount = a.payment_type == "MER_TRANSFER_SYS"
                                                //                                         ? (p.payment_type == "Cash" ? (p.return_type == "Point" ? a.amount - expense_fee : a.amount - (expense_fee * 2))
                                                //                                             : (p.return_type == "Point"
                                                //                                             ? (a.amount > amount_limit ? a.amount - collection_fee - expense_fee : a.amount - collection_fee)
                                                //                                             : (a.amount > amount_limit ? a.amount - expense_fee - (collection_fee * 2) : a.amount - expense_fee - collection_fee)))
                                                //                                         : a.amount,
                                                 amount = p.payment_type == "BaoKim" && a.payment_type == "MER_TRANSFER_SYS" ? (p.return_type == "Point" ? 
                                                                             (a.amount > amount_limit ? a.amount - collection_fee - expense_fee : a.amount - collection_fee)
                                                                             :(a.amount > amount_limit ? a.amount - collection_fee - (expense_fee * 2) : a.amount - expense_fee - collection_fee) ) : a.amount,
                                                 payment_type = a.payment_type,
                                                 transaction_no = a.transaction_no,
                                                 bao_kim_transaction_id = a.bao_kim_transaction_id,
                                                 transaction_date = a.transaction_date,
                                                 bank_receive_name = a.bank_receive_name,
                                                 bank_receive_account = a.bank_receive_account,
                                                 bank_receive_owner = a.bank_receive_owner,
                                                 trans_status = a.trans_status,
                                                 trans_log = a.trans_log,
                                                 transaction_description = a.transaction_description,
                                                 accumulate_point_order_id = a.accumulate_point_order_id,
                                                 partner_id = a.partner_id,
                                                 customer_id = a.customer_id,
                                                 view = a.amount > amount_limit ? true : false
                                             })
                                               .ToList(),
                            list_affiliates = (from a in _context.AccumulatePointOrderAffiliates
                                               where a.accumulate_point_order_id == p.id
                                               select new
                                               {
                                                   levels = a.levels,
                                                   discount_rate = a.discount_rate,
                                                   username = a.username,
                                                   date_created = a.date_created != null ? _commonFunction.convertDateToStringFull(a.date_created) : "",
                                                   point_value = a.point_value
                                               }).ToList(),
                            amount_balance = _context.BaoKimTransactions.Where(x => x.accumulate_point_order_id == p.id && x.amount_balance != null).Select(p => p.amount_balance).FirstOrDefault(),
                            collection_fee = collection_fee,
                            expense_fee = expense_fee,
                            amount_limit = amount_limit
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            return new APIResponse(data);
        }
    }
}
