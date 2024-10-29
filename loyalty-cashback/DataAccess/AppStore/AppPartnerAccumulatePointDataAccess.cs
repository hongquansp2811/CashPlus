using LOYALTY.CloudMessaging;
using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using LOYALTY.PaymentGate;
using LOYALTY.PaymentGate.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LOYALTY.DataAccess
{
    public class AppPartnerAccumulatePointOrderDataAccess : IPartnerAccumulatePointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly ICommon _common;
        private readonly FCMNotification _fCMNotification;
        private readonly BKTransaction _bkTransaction;
        private static JsonSerializerOptions option;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISendSMSBrandName _sendSMSBrandName;
        public AppPartnerAccumulatePointOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction, ICommon common, FCMNotification fCMNotification, BKTransaction bkTransaction,
            ISendSMSBrandName sendSMSBrandName, IServiceScopeFactory serviceScopeFactory)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _common = common;
            _fCMNotification = fCMNotification;
            _bkTransaction = bkTransaction;
            option = new JsonSerializerOptions { WriteIndented = true };
            _sendSMSBrandName = sendSMSBrandName;
            _serviceScopeFactory = serviceScopeFactory;
        }


        public APIResponse getCustomerDetailByQR(Guid id, Guid partner_id)
        {
            var startDate = DateTime.Now.Date;
            var endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            var data = (from p in _context.Customers
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            full_name = p.full_name,
                            phone = p.phone,
                            total_scan_qr = _context.AccumulatePointOrders.Where(x => x.customer_id == p.id && x.partner_id == partner_id && (x.approve_date != null && x.approve_date >= startDate && x.approve_date <= endDate)).Count(),
                            list_today_orders = _context.AccumulatePointOrders.Where(x => x.customer_id == p.id && x.partner_id == partner_id && (x.approve_date != null && x.approve_date >= startDate && x.approve_date <= endDate)).Select(x => new
                            {
                                bill_amount = x.bill_amount,
                                approve_date = _commonFunction.convertDateToStringFull(x.approve_date)
                            }).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getList(AccumulatePointOrderRequest request)
        {
            var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;

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
                           join s in _context.Partners on p.partner_id equals s.id
                           join c in _context.Customers on p.customer_id equals c.id
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = p.date_created,
                               trans_date_origin = p.date_created,
                               partner_name = s.name,
                               customer_name = c.full_name,
                               customer_phone = c.phone,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_partner,
                               amount_exchange = Math.Round((decimal)p.point_partner * pointExchangeRate),
                               description = p.description,
                               approve_user = p.approve_user,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).AddHours(23).AddMinutes(59));
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

            var startDate = DateTime.Now.Date;
            var endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            var data = (from p in _context.AccumulatePointOrders
                        join s in _context.Partners on p.partner_id equals s.id
                        join c in _context.Customers on p.customer_id equals c.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        join com in _context.AccumulatePointOrderComplains on p.id equals com.accumulate_order_id into coms
                        from com in coms.DefaultIfEmpty()
                        join ra in _context.AccumulatePointOrderRatings on p.id equals ra.accumulate_point_order_id into ras
                        from ra in ras.DefaultIfEmpty()
                        join appu in _context.Users on p.user_created equals appu.username into appus
                        from appu in appus.DefaultIfEmpty()
                        where p.id == id && appu.is_customer == false
                        select new
                        {
                            id = p.id,
                            trans_no = p.trans_no,
                            trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                            partner_id = p.partner_id,
                            partner_name = s.name,
                            partner_address = s.address,
                            customer_id = p.customer_id,
                            customer_name = c.full_name,
                            customer_phone = c.phone,
                            total_scan_qr = _context.AccumulatePointOrders.Where(x => x.customer_id == p.customer_id && x.partner_id == p.partner_id && (x.approve_date != null && x.approve_date >= startDate && x.approve_date <= endDate)).Count(),
                            list_today_orders = _context.AccumulatePointOrders.Where(x => x.customer_id == p.customer_id && x.partner_id == p.partner_id && (x.approve_date != null && x.approve_date >= startDate && x.approve_date <= endDate)).Select(x => new
                            {
                                bill_amount = x.bill_amount,
                                approve_date = x.approve_date != null ? _commonFunction.convertDateToStringFull(x.approve_date) : "",
                                date_created = x.date_created
                            }).OrderBy(x => x.date_created).ToList(),
                            bill_amount = p.bill_amount,
                            point_exchange = p.point_partner,
                            amount_partner = Math.Round((decimal)p.point_partner * pointExchangeRate),
                            amount_customer = Math.Round((decimal)p.point_customer * pointExchangeRate),
                            amount_other = Math.Round((decimal)(p.point_partner - p.point_customer) * pointExchangeRate),
                            description = p.description,
                            approve_user = appu != null ? appu.username : "",
                            approve_user_fullname = appu != null ? appu.full_name : "",
                            status = p.status,
                            is_complain = com != null ? true : false,
                            is_review = ra != null ? true : false,
                            status_name = st != null ? st.name : ""
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getListCMS(AccumulatePointOrderRequest request)
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
                           where p.partner_id == request.partner_id
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

        public APIResponse getDetailCMS(Guid id)
        {
            var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;

            decimal collection_fee = settings != null && settings.collection_fee != null && settings.collection_fee != 0 ? (decimal)settings.collection_fee : 1; //thu hộ
            decimal expense_fee = settings != null && settings.expense_fee != null && settings.expense_fee != 0 ? (decimal)settings.expense_fee : 1; //Chi hộ
            decimal amount_limit = settings != null && settings.amount_limit != null && settings.amount_limit != 0 ? (decimal)settings.amount_limit : 1;

            var AccumulatePointOrderDetails = _context.AccumulatePointOrderDetails.ToList();

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
                            approve_user = p.approve_user,
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
                            point_value_count = _context.AccumulatePointOrderDetails.Where(x => x.accumulate_point_order_id == p.id && x.allocation_name != "Người tiêu dùng" && x.point_value >= 0).Sum(x => x.point_value),
                            list_details = _context.AccumulatePointOrderDetails.Where(x => x.accumulate_point_order_id == p.id).ToList(),
                            //list_bk_trans = _context.BaoKimTransactions.Where(x => x.accumulate_point_order_id == p.id).ToList(),
                            list_bk_trans = (from a in _context.BaoKimTransactions
                                             where a.accumulate_point_order_id == p.id
                                             select new
                                             {
                                                 id = a.id,
                                                 amount2 = a.amount,
                                                //  amount = p.payment_type == "Cash" ? (p.return_type == "Point" ? a.amount - expense_fee : a.amount - (expense_fee * 2))
                                                //                                     : (p.return_type == "Point"
                                                //                                         ? (a.amount > amount_limit ? a.amount - collection_fee - expense_fee : a.amount - collection_fee)
                                                //                                         : (a.amount > amount_limit ? a.amount - expense_fee - (collection_fee * 2) : a.amount - expense_fee - collection_fee)),
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
                                             }).ToList(),
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

        public APIResponse denied(Guid id, string username)
        {

            var request = _context.AccumulatePointOrders.Where(x => x.id == id).FirstOrDefault();

            if (request == null)
            {
                return new APIResponse("ERROR_ORDER_ID_NOT_EXISTS");
            }

            var customerObj = _context.Customers.Where(x => x.id == request.customer_id).FirstOrDefault();
            if (customerObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_NOT_EXISTS");
            }

            var userCustomerObj = _context.Users.Where(x => x.customer_id == request.customer_id && x.is_customer == true).FirstOrDefault();
            if (userCustomerObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_NOT_EXISTS");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                request.status = 6;
                _context.SaveChanges();

                if (userCustomerObj.device_id != null)
                {
                    // Gửi FCM cho tài khoản phát triển cộng đồng
                    _fCMNotification.SendNotification(userCustomerObj.device_id,
                    "ACCU_ORDER",
                    "Từ chối xác nhận tích điểm",
                    "Bạn vừa bị từ chối xác nhận đơn hàng",
                    null);

                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Từ chối xác nhận tích điểm";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userCustomerObj.customer_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    newNoti1.content = "Bạn vừa bị từ chối xác nhận đơn hàng";
                    newNoti1.system_type = "ACCU_ORDER";
                    newNoti1.reference_id = null;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(400);
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }

        public APIResponse confirm(Guid id, string username)
        {

            var request = _context.AccumulatePointOrders.Where(x => x.id == id).FirstOrDefault();

            if (request == null)
            {
                return new APIResponse("ERROR_ORDER_ID_NOT_EXISTS");
            }

            var dateNow = DateTime.Now;

            //if (request.date_created.Value.AddMinutes(20) <= dateNow)
            //{
            //    return new APIResponse("ERROR_ORDER_EXPIRE");
            //}

            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.bill_amount == null)
            {
                return new APIResponse("ERROR_BILL_AMOUNT_MISSING");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_EXISTS");
            }

            if (partnerObj.status != 15)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_AVAIABLE");
            }

            if (partnerObj.bk_partner_code == null)
            {
                return new APIResponse("ERROR_PARTNER_BAOKIM_NOT_AVAIABLE");
            }

            var userPartner = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == request.partner_id).FirstOrDefault();

            if (userPartner == null)
            {
                return new APIResponse("ERROR_USER_PARTNER_NOT_EXISTS");
            }

            var contractObj = _context.PartnerContracts.Where(x => x.status == 12 && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id).FirstOrDefault();
            if (contractObj == null)
            {
                return new APIResponse("ERROR_CONTRACT_NOT_EXISTS");
            }

            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null)
            {
                return new APIResponse("ERROR_SETTINGS_NOT_CONFIG");
            }

            var customerObj = _context.Customers.Where(x => x.id == request.customer_id).FirstOrDefault();
            if (customerObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_NOT_EXISTS");
            }

            CustomerBankAccount customerBankAccountObj = new CustomerBankAccount();

            customerBankAccountObj = _context.CustomerBankAccounts.Where(x => x.user_id == customerObj.id && x.is_default == true).FirstOrDefault();
            if (customerBankAccountObj == null)
            {
                customerBankAccountObj = _context.CustomerBankAccounts.Where(x => x.user_id == customerObj.id).FirstOrDefault();
                if (customerBankAccountObj == null)
                {
                    return new APIResponse("ERROR_CUSTOMER_BANK_ACCOUNT_NOT_EXISTS");
                }
            }

            var customerBankObj = _context.Banks.Where(x => x.id == customerBankAccountObj.bank_id).FirstOrDefault();
            if (customerBankObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_BANK_NOT_EXISTS");
            }

            var sysBankObj = _context.Banks.Where(x => x.id == settingObj.sys_receive_bank_id).FirstOrDefault();
            if (sysBankObj == null)
            {
                return new APIResponse("ERROR_SYSTEM_BANK_NOT_EXISTS");
            }

            var userCustomer = _context.Users.Where(x => x.is_customer == true && x.customer_id == request.customer_id).FirstOrDefault();
            if (userCustomer == null)
            {
                return new APIResponse("ERROR_USER_CUSTOMER_NOT_EXISTS");
            }

            decimal pointExchangeRate = Math.Round(((decimal)settingObj.point_exchange / (decimal)settingObj.point_value), 2);
            decimal customerExchange = 0;
            decimal affiliateExchange = 0;
            decimal systemExchange = 0;
            // Lấy cấu hình đổi điểm hiệu lực
            var accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id && x.status == 23).FirstOrDefault();

            // Nếu không có riêng thì lấy chung
            if (accumulateConfig == null)
            {
                accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();
            }

            if (accumulateConfig == null)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfig.id).ToList();

            if (listAccuDetails.Count == 0)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            for (int i = 0; i < listAccuDetails.Count; i++)
            {
                if (listAccuDetails[i].allocation_name == "Khách hàng")
                {
                    customerExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Điểm tích lũy")
                {
                    affiliateExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Hệ thống")
                {
                    systemExchange = (decimal)listAccuDetails[i].discount_rate;
                }
            }
            //Tổng thanh toán
            decimal total_point = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate * pointExchangeRate / 100);

            bool isDebit = false;
            //Tính điểm 
            decimal pointCustomer = Math.Round(total_point * customerExchange / 100);
            decimal pointAffiliate = Math.Round(total_point * affiliateExchange / 100);
            decimal pointSystem = Math.Round(total_point * systemExchange / 100);

            var total_scan_qr = _context.AccumulatePointOrders.Where(x => x.partner_id == request.partner_id && x.customer_id == request.customer_id).Count();

            var min_exchange_transaction = settingObj.cash_condition_value;
            var total_allow_cash = settingObj.total_allow_cash;

            var total_accumulate = _context.AccumulatePointOrders.Where(x => x.customer_id == request.customer_id && x.approve_date != null).Count();

            long amount_balance = 0;

            if (partnerObj.bk_partner_code != null)
            {
                //Kiểm tra số dư
                GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

                amount_balance = balanceObj.Available;
            }

            decimal total_money = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate / 100);


            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Nếu không đủ điều kiện hoàn tiền thì cộng điểm
                if (total_accumulate > total_allow_cash && ((pointCustomer * pointExchangeRate) < min_exchange_transaction))
                {
                    request.return_type = "Point";
                    // Nếu thanh toán tiền mặt/Chuyển 100% từ deposit merchant sang CashPlus
                    if (request.payment_type == "Cash")
                    {
                        if (amount_balance < total_money)
                        {
                            throw new Exception("ERROR_PARTNER_ENOUGH_AMOUNT");
                        }

                        //TransferResponseObj response = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", (total_point * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                        TransferResponseObj response = _bkTransaction.transferMoney(partnerObj.bk_partner_code, sysBankObj.bank_code, settingObj.sys_receive_bank_no, settingObj.sys_receive_bank_owner, (total_point * pointExchangeRate), "Chuyen tien tai BK voi ma don hang " + request.trans_no, partnerObj.RSA_privateKey);

                        // Log giao dịch chuyển cho NTD
                        BaoKimTransaction ntdTrans = new BaoKimTransaction();
                        ntdTrans.id = Guid.NewGuid();
                        ntdTrans.payment_type = "MER_TRANSFER_SYS";
                        ntdTrans.bao_kim_transaction_id = response.TransactionId;
                        ntdTrans.transaction_no = response.ReferenceId;
                        ntdTrans.amount = (total_point * pointExchangeRate);
                        ntdTrans.accumulate_point_order_id = request.id;
                        ntdTrans.partner_id = request.partner_id;
                        ntdTrans.customer_id = request.customer_id;

                        ntdTrans.bank_receive_name = sysBankObj.name;
                        ntdTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                        ntdTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                        ntdTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                        if (response.ResponseCode == 200)
                        {
                            ntdTrans.trans_status = 25;
                        }
                        else
                        {
                            ntdTrans.trans_status = 26;
                        }
                        ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                        ntdTrans.transaction_date = DateTime.Now;

                        _context.BaoKimTransactions.Add(ntdTrans);
                        _context.SaveChanges();
                    }
                    else
                    { // Nếu thanh toán online thì tạo lệnh chờ giao dịch
                        // Log giao dịch chuyển cho Hệ thống
                        BaoKimTransaction sysTrans = new BaoKimTransaction();
                        sysTrans.id = Guid.NewGuid();
                        sysTrans.payment_type = "MER_TRANSFER_SYS";
                        sysTrans.bao_kim_transaction_id = "";
                        sysTrans.transaction_no = request.trans_no;
                        sysTrans.amount = (total_point * pointExchangeRate);
                        sysTrans.accumulate_point_order_id = request.id;
                        sysTrans.partner_id = request.partner_id;
                        sysTrans.customer_id = request.customer_id;

                        sysTrans.bank_receive_name = customerBankObj.name;
                        sysTrans.bank_receive_account = customerBankAccountObj.bank_no;
                        sysTrans.bank_receive_owner = customerBankAccountObj.bank_owner;
                        sysTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                        sysTrans.trans_status = 27;
                        //ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                        sysTrans.transaction_date = DateTime.Now;

                        _context.BaoKimTransactions.Add(sysTrans);
                        _context.SaveChanges();
                    }

                    // Cộng điểm khách hàng
                    var newCustomerPointHistory = new CustomerPointHistory();
                    newCustomerPointHistory.id = Guid.NewGuid();
                    newCustomerPointHistory.order_type = "PUSH";
                    newCustomerPointHistory.customer_id = request.customer_id;
                    newCustomerPointHistory.point_amount = pointCustomer;
                    newCustomerPointHistory.status = 4;
                    newCustomerPointHistory.trans_date = DateTime.Now;
                    userCustomer.point_avaiable += pointCustomer;
                    newCustomerPointHistory.point_type = "AVAIABLE";
                    _context.CustomerPointHistorys.Add(newCustomerPointHistory);

                    userCustomer.total_point = userCustomer.point_affiliate + userCustomer.point_avaiable + userCustomer.point_waiting;
                    _context.SaveChanges();
                    _common.checkUpdateCustomerRank((Guid)userCustomer.id);
                }
                else
                {
                    // Đủ diều kiện hoàn tiền
                    request.return_type = "Cash";

                    if (request.payment_type == "Cash")
                    { // Nếu giao dịch tiền mặt
                        if (amount_balance < total_money)
                        {
                            throw new Exception("ERROR_PARTNER_ENOUGH_AMOUNT");
                        }
                        // Chuyển khoản từ deposit merchant sang NTD CK Merchant
                        //TransferResponseObj response = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", (pointCustomer * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                        TransferResponseObj response = _bkTransaction.transferMoney(partnerObj.bk_partner_code, customerBankObj.bank_code, customerBankAccountObj.bank_no, customerBankAccountObj.bank_owner, (pointCustomer * pointExchangeRate), "Chuyen tien tai BK voi ma don hang " + request.trans_no, partnerObj.RSA_privateKey);

                        // Log giao dịch chuyển cho NTD
                        BaoKimTransaction ntdTrans = new BaoKimTransaction();
                        ntdTrans.id = Guid.NewGuid();
                        ntdTrans.payment_type = "MER_TRANSFER_NTD";
                        ntdTrans.bao_kim_transaction_id = response.TransactionId;
                        ntdTrans.transaction_no = response.ReferenceId;
                        ntdTrans.amount = (pointCustomer * pointExchangeRate);
                        ntdTrans.accumulate_point_order_id = request.id;
                        ntdTrans.partner_id = request.partner_id;
                        ntdTrans.customer_id = request.customer_id;

                        ntdTrans.bank_receive_name = customerBankObj.name;
                        ntdTrans.bank_receive_account = customerBankAccountObj.bank_no;
                        ntdTrans.bank_receive_owner = customerBankAccountObj.bank_owner;
                        ntdTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                        if (response.ResponseCode == 200)
                        {
                            ntdTrans.trans_status = 25;
                        }
                        else
                        {
                            ntdTrans.trans_status = 26;
                        }
                        ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                        ntdTrans.transaction_date = DateTime.Now;

                        _context.BaoKimTransactions.Add(ntdTrans);
                        _context.SaveChanges();

                        // Chuyển khoản từ deposit merchant sang CashPlus CK CashPLus + Affiliate
                        //TransferResponseObj response2 = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", ((pointSystem + pointAffiliate) * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                        TransferResponseObj response2 = _bkTransaction.transferMoney(partnerObj.bk_partner_code, sysBankObj.bank_code, settingObj.sys_receive_bank_no, settingObj.sys_receive_bank_owner, ((pointSystem + pointAffiliate) * pointExchangeRate), "Chuyen tien tai BK voi ma don hang " + request.trans_no, partnerObj.RSA_privateKey);

                        // Log giao dịch chuyển sang cho CashPlus
                        BaoKimTransaction sysTrans = new BaoKimTransaction();
                        sysTrans.id = Guid.NewGuid();
                        sysTrans.payment_type = "MER_TRANSFER_SYS";
                        sysTrans.bao_kim_transaction_id = response2.TransactionId;
                        sysTrans.transaction_no = response2.ReferenceId;
                        sysTrans.amount = ((pointAffiliate + pointSystem) * pointExchangeRate);
                        sysTrans.accumulate_point_order_id = request.id;
                        sysTrans.partner_id = request.partner_id;
                        sysTrans.customer_id = request.customer_id;
                        sysTrans.transaction_description = "Chuyen tien tai BK voi ma don hang " + request.trans_no;

                        sysTrans.bank_receive_name = sysBankObj.name;
                        sysTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                        sysTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                        if (response2.ResponseCode == 200)
                        {
                            sysTrans.trans_status = 25;
                        }
                        else
                        {
                            sysTrans.trans_status = 26;
                        }
                        sysTrans.transaction_date = DateTime.Now;
                        sysTrans.trans_log = JsonSerializer.Serialize(response2, option);

                        _context.BaoKimTransactions.Add(sysTrans);
                        _context.SaveChanges();
                    }
                    else
                    {
                        // Nếu giao dịch online
                        // Log giao dịch cho NTD
                        // Chuyển tiền từ CashPlus cho NTD
                        //TransferResponseObj response = _bkTransaction.transferMoney(Consts.CP_BK_PARTNER_CODE, "970409", "9704060224009513", "Nguyen Van A", (pointCustomer * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                        TransferResponseObj response = _bkTransaction.transferMoney(Consts.CP_BK_PARTNER_CODE, customerBankObj.bank_code, customerBankAccountObj.bank_no, customerBankAccountObj.bank_owner, (pointCustomer * pointExchangeRate), "Chuyen tien tai BK voi ma don hang " + request.trans_no, Consts.private_key);

                        // Log giao dịch chuyển cho NTD
                        BaoKimTransaction ntdTrans = new BaoKimTransaction();
                        ntdTrans.id = Guid.NewGuid();
                        ntdTrans.payment_type = "CP_TRANSFER_NTD";
                        ntdTrans.bao_kim_transaction_id = response.TransactionId;
                        ntdTrans.transaction_no = response.ReferenceId;
                        ntdTrans.amount = (pointCustomer * pointExchangeRate);
                        ntdTrans.accumulate_point_order_id = request.id;
                        ntdTrans.partner_id = request.partner_id;
                        ntdTrans.customer_id = request.customer_id;

                        ntdTrans.bank_receive_name = customerBankObj.name;
                        ntdTrans.bank_receive_account = customerBankAccountObj.bank_no;
                        ntdTrans.bank_receive_owner = customerBankAccountObj.bank_owner;
                        ntdTrans.transaction_description = "Chuyen tien tai BK voi ma don hang " + request.trans_no;

                        if (response.ResponseCode == 200)
                        {
                            ntdTrans.trans_status = 25;
                        }
                        else
                        {
                            ntdTrans.trans_status = 26;
                        }
                        ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                        ntdTrans.transaction_date = DateTime.Now;

                        _context.BaoKimTransactions.Add(ntdTrans);
                        _context.SaveChanges();

                        // Log giao dịch chuyển cho Hệ thống
                        BaoKimTransaction sysTrans = new BaoKimTransaction();
                        sysTrans.id = Guid.NewGuid();
                        sysTrans.payment_type = "TRANSFER_SYS";
                        sysTrans.bao_kim_transaction_id = "";
                        sysTrans.transaction_no = request.trans_no;
                        sysTrans.amount = (total_point * pointExchangeRate);
                        sysTrans.accumulate_point_order_id = request.id;
                        sysTrans.partner_id = request.partner_id;
                        sysTrans.customer_id = request.customer_id;

                        sysTrans.bank_receive_name = customerBankObj.name;
                        sysTrans.bank_receive_account = customerBankAccountObj.bank_no;
                        sysTrans.bank_receive_owner = customerBankAccountObj.bank_owner;
                        sysTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                        sysTrans.trans_status = 27;
                        //ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                        ntdTrans.transaction_date = DateTime.Now;

                        _context.BaoKimTransactions.Add(sysTrans);
                        _context.SaveChanges();
                    }
                }

                _context.SaveChanges();

                // Xử lý điểm Affiliate
                decimal pointSystemAffiliate = pointAffiliate;
                string usernameLv1 = "";
                if (pointSystemAffiliate >= 0 && userCustomer.share_person_id != null)
                {
                    var userLv1 = _context.Users.Where(x => x.id == userCustomer.share_person_id).FirstOrDefault();

                    if (userLv1 != null && userLv1.is_delete != true && userLv1.status == 1)
                    {
                        usernameLv1 = userLv1.username;
                        userLv1.point_affiliate += pointSystemAffiliate;
                        userLv1.total_point = userLv1.point_affiliate + userLv1.point_avaiable + userLv1.point_waiting;

                        if (userLv1.is_customer == true)
                        {
                            var newCustomerPointHistoryLv1 = new CustomerPointHistory();
                            newCustomerPointHistoryLv1.id = Guid.NewGuid();
                            newCustomerPointHistoryLv1.order_type = "AFF_LV_1";
                            newCustomerPointHistoryLv1.customer_id = userLv1.customer_id;
                            newCustomerPointHistoryLv1.source_id = request.customer_id;
                            newCustomerPointHistoryLv1.point_amount = pointSystemAffiliate;
                            newCustomerPointHistoryLv1.status = 4;
                            newCustomerPointHistoryLv1.trans_date = DateTime.Now;
                            newCustomerPointHistoryLv1.point_type = "WAITING";
                            _context.CustomerPointHistorys.Add(newCustomerPointHistoryLv1);
                        }
                        else if (userLv1.is_partner == true)
                        {
                            var newPartnerPointHistoryLv1 = new PartnerPointHistory();
                            newPartnerPointHistoryLv1.id = Guid.NewGuid();
                            newPartnerPointHistoryLv1.order_type = "AFF_LV_1";
                            newPartnerPointHistoryLv1.partner_id = userLv1.partner_id;
                            newPartnerPointHistoryLv1.source_id = request.customer_id;
                            newPartnerPointHistoryLv1.point_amount = pointSystemAffiliate;
                            newPartnerPointHistoryLv1.status = 4;
                            newPartnerPointHistoryLv1.trans_date = DateTime.Now;
                            newPartnerPointHistoryLv1.point_type = "WAITING";
                            _context.PartnerPointHistorys.Add(newPartnerPointHistoryLv1);
                        }

                        _context.SaveChanges();

                        var newAffi = new AccumulatePointOrderAffiliate();
                        newAffi.id = Guid.NewGuid();
                        newAffi.accumulate_point_order_id = request.id;
                        newAffi.username = usernameLv1.Length > 0 ? usernameLv1 : "Hệ thống";
                        newAffi.discount_rate = affiliateExchange;
                        newAffi.date_created = DateTime.Now;
                        newAffi.levels = 1;
                        newAffi.point_value = pointSystemAffiliate;

                        _context.AccumulatePointOrderAffiliates.Add(newAffi);
                        _context.SaveChanges();

                        if (userLv1.device_id != null)
                        {
                            // Gửi FCM cho tài khoản phát triển cộng đồng
                            _fCMNotification.SendNotification(userLv1.device_id,
                            "WALLET",
                            "Điểm phát triển cộng đồng",
                            "Tài khoản của bạn đã nhận được " + pointAffiliate.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ việc phát triển cộng đồng",
                            null);

                            var newNoti1 = new Notification();
                            newNoti1.id = Guid.NewGuid();
                            newNoti1.title = "Điểm phát triển cộng đồng";
                            newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                            newNoti1.user_id = userLv1.is_partner == true ? userLv1.partner_id : userLv1.customer_id;
                            newNoti1.date_created = DateTime.Now;
                            newNoti1.date_updated = DateTime.Now;
                            newNoti1.content = "Tài khoản của bạn đã nhận được " + pointAffiliate.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ việc phát triển cộng đồng";
                            newNoti1.system_type = "WALLET";
                            newNoti1.reference_id = null;

                            _context.Notifications.Add(newNoti1);
                            _context.SaveChanges();
                        }
                    }
                }

                // Cộng điểm hệ thống
                var newSystemPointHistory = new SystemPointHistory();
                newSystemPointHistory.id = Guid.NewGuid();
                newSystemPointHistory.order_type = "PUSH";
                newSystemPointHistory.point_amount = pointSystem;
                newSystemPointHistory.status = 4;
                newSystemPointHistory.trans_date = DateTime.Now;

                if (isDebit == true)
                {
                    newSystemPointHistory.point_type = "WAITING";
                }
                else
                {
                    newSystemPointHistory.point_type = "AVAIABLE";
                }

                _context.SystemPointHistorys.Add(newSystemPointHistory);

                _context.SaveChanges();


                // Lưu thông tin điểm hóa đơn
                request.status = 5;
                request.discount_rate = contractObj.discount_rate;
                request.point_exchange = total_point;
                if (isDebit == true)
                {
                    request.point_waiting = total_point;
                    request.point_avaiable = 0;
                }
                else
                {
                    request.point_avaiable = total_point;
                    request.point_waiting = 0;
                }
                request.point_partner = total_point;
                request.point_customer = pointCustomer;
                request.point_system = pointSystem;
                request.approve_user = username;
                request.user_updated = username;
                request.approve_date = DateTime.Now;
                request.date_updated = DateTime.Now;
                _context.SaveChanges();

                // Tạo detail
                for (int i = 0; i < listAccuDetails.Count; i++)
                {
                    var newDetail = new AccumulatePointOrderDetail();
                    newDetail.id = Guid.NewGuid();
                    newDetail.accumulate_point_order_id = request.id;
                    newDetail.name = listAccuDetails[i].name;
                    newDetail.discount_rate = listAccuDetails[i].discount_rate;
                    if (listAccuDetails[i].name == "Khách hàng")
                    {
                        newDetail.point_value = pointCustomer;
                        newDetail.allocation_name = "Người tiêu dùng";
                        newDetail.description = "Chiết khấu cho NTD";
                    }
                    else if (listAccuDetails[i].name == "Hệ thống")
                    {
                        newDetail.point_value = pointSystem;
                        newDetail.allocation_name = "Hệ thống(Admin)";
                        newDetail.description = "Chiết khấu cho CashPlus";
                    }
                    else if (listAccuDetails[i].name == "Điểm tích lũy")
                    {
                        newDetail.point_value = pointAffiliate;
                        newDetail.allocation_name = "Điểm tích lũy";
                        newDetail.description = "Chiết khấu cho người giưới thiệu sẽ được chuyển về CashPlus, từ đó CashPlus sẽ cộng điểm tích lũy cho NGT";
                    }

                    _context.AccumulatePointOrderDetails.Add(newDetail);
                }

                _context.SaveChanges();

                // Gửi FCM
                // Cho khách
                if (userCustomer.device_id != null)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userCustomer.customer_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    if (request.return_type == "CASH")
                    {
                        newNoti1.title = "Hoàn tiền tiêu dùng";
                        newNoti1.content = "Tài khoản của bạn vừa nhận được " + (pointCustomer * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " VNĐ từ giao dịch " + request.trans_no + " với số tiền thanh toán là: " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " vào lúc " + _commonFunction.convertDateToStringFull(request.date_created);
                    }
                    else
                    {
                        newNoti1.title = "Tích điểm tiêu dùng";
                        newNoti1.content = "Tài khoản của bạn vừa nhận được " + pointCustomer.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ giao dịch " + request.trans_no + " với số tiền thanh toán là: " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " vào lúc " + _commonFunction.convertDateToStringFull(request.date_created);
                    }
                    newNoti1.system_type = "ACCU_POINT";
                    newNoti1.reference_id = request.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    _fCMNotification.SendNotification(userCustomer.device_id,
                            "ACCU_POINT",
                            newNoti1.title,
                           newNoti1.content,
                            request.id);
                }

                // Cho shop
                if (userPartner.device_id != null)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Hoàn tiền tiêu dùng";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userPartner.partner_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    newNoti1.content = "Tài khoản của bạn đã bị trừ " + (total_point * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + "VNĐ từ giao dịch " + request.trans_no + " xác nhậm thanh toán số tiền " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." });
                    newNoti1.system_type = "ACCU_POINT";
                    newNoti1.reference_id = request.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    _fCMNotification.SendNotification(userPartner.device_id,
                            "ACCU_POINT",
                            newNoti1.title,
                            newNoti1.content,
                            request.id);
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(ex.Message);
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(new
            {
                total_scan_qr = total_scan_qr != null ? (total_scan_qr + 1) : 1
            });
        }

        public APIResponse create(AccumulatePointOrderRequest request, string username)
        {
            var dateNow = DateTime.Now;

            var payment_limit = _context.Settingses.Select(p => p.payment_limit).FirstOrDefault();

            if (request.bill_amount < payment_limit)
            {
                var notiReturn = _context.NotiConfigs.FirstOrDefault();
                return new APIResponse(notiReturn.MC_amount_bill.Replace("{Config_amount_bill}", ((decimal)payment_limit).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })));
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.bill_amount == null)
            {
                return new APIResponse("ERROR_BILL_AMOUNT_MISSING");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_EXISTS");
            }

            if (partnerObj.status != 15)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_AVAIABLE");
            }

            var userPartner = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == request.partner_id).FirstOrDefault();

            if (userPartner == null)
            {
                return new APIResponse("ERROR_USER_PARTNER_NOT_EXISTS");
            }

            var contractObj = _context.PartnerContracts.Where(x => x.status == 12 && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id).FirstOrDefault();
            if (contractObj == null)
            {
                return new APIResponse("ERROR_CONTRACT_NOT_EXISTS");
            }

            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null)
            {
                return new APIResponse("ERROR_SETTINGS_NOT_CONFIG");
            }

            decimal pointExchangeRate = Math.Round(((decimal)settingObj.point_exchange / (decimal)settingObj.point_value), 2);
            decimal customerExchange = 0;
            decimal affiliateExchange = 0;
            decimal systemExchange = 0;
            // Lấy cấu hình đổi điểm hiệu lực
            var accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id && x.status == 23).FirstOrDefault();

            // Nếu không có riêng thì lấy chung
            if (accumulateConfig == null)
            {
                accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();
            }

            if (accumulateConfig == null)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfig.id).ToList();

            if (listAccuDetails.Count == 0)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            for (int i = 0; i < listAccuDetails.Count; i++)
            {
                if (listAccuDetails[i].allocation_name == "Khách hàng")
                {
                    customerExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Điểm tích lũy")
                {
                    affiliateExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Hệ thống")
                {
                    systemExchange = (decimal)listAccuDetails[i].discount_rate;
                }
            }

            decimal total_point = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate * pointExchangeRate / 100);

            // long amount_balance = 0;

            //if (partnerObj.bk_partner_code != null)
            //{
            //    //Check số dư
            //    GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

            //    amount_balance = balanceObj.Available;
            //}

            decimal total_money = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate / 100);

            //if (amount_balance < total_money)
            //{
            //    string notiReturn = _context.NotiConfigs.Select(p => p.MC_Payment_CheckSurplus).FirstOrDefault();
            //    string customerName = _context.Customers.Where(l => l.id == request.customer_id).Select(p => p.full_name).FirstOrDefault();
            //    return new APIResponse(notiReturn.Replace("{SoDu}", amount_balance.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{TongChietKhau}", total_money.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{CustomerName}", customerName));
            //}

            var transaction = _context.Database.BeginTransaction();

            var data = new AccumulatePointOrder();
            Guid orderId = Guid.NewGuid();
            try
            {
                // Xử lý điểm Affiliate
                decimal pointAffiliate = Math.Round(total_point * affiliateExchange / 100);
                decimal pointCustomer = Math.Round(total_point * customerExchange / 100);
                decimal pointSystem = Math.Round(total_point * systemExchange / 100);

                // Tạo hóa đơn tích điểm
                var maxCodeObject = _context.AccumulatePointOrders.Where(x => x.trans_no != null && x.trans_no.Contains("TD")).OrderByDescending(x => x.trans_no).FirstOrDefault();
                string codeOrder = "";
                if (maxCodeObject == null)
                {
                    codeOrder = "TD0000000000001";
                }
                else
                {
                    string maxCode = maxCodeObject.trans_no;
                    maxCode = maxCode.Substring(2);
                    int orders = int.Parse(maxCode);
                    orders = orders + 1;
                    string orderString = orders.ToString();
                    char pad = '0';
                    int number = 13;
                    codeOrder = "TD" + orderString.PadLeft(number, pad);
                }

                data.id = orderId;
                data.customer_id = request.customer_id;
                data.partner_id = request.partner_id;
                data.trans_no = codeOrder;
                data.description = request.description;
                data.bill_amount = request.bill_amount;
                data.payment_type = "Cash";
                data.return_type = "Point";
                data.point_exchange = 0;
                data.point_customer = 0;
                data.point_partner = total_money;
                data.point_system = 0;
                data.description = request.description;
                data.status = 4;
                data.discount_rate = contractObj.discount_rate;
                data.address = partnerObj.address;
                data.approve_user = username;
                data.user_updated = username;
                data.approve_date = DateTime.Now;
                data.date_updated = DateTime.Now;
                data.user_created = username;
                data.date_created = DateTime.Now;
                _context.AccumulatePointOrders.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(ex.InnerException + "-" + ex.Message);
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(data);
        }

        //Thanh toán tiền mặt
        public APIResponse CashPayment(Guid id, string username)
        {
            var request = _context.AccumulatePointOrders.Where(x => x.id == id).FirstOrDefault();

            if (request.status != 4)
            {
                return new APIResponse("Đơn hàng đã được thanh toán");
            }

            if (request == null)
            {
                return new APIResponse("ERROR_ORDER_ID_NOT_EXISTS");
            }

            var dateNow = DateTime.Now;

            if (request.date_created.Value.AddMinutes(20) <= dateNow)
            {
                request.status = 6;
                _context.SaveChanges();
                return new APIResponse("ERROR_ORDER_EXPIRE");
            }

            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.bill_amount == null)
            {
                return new APIResponse("ERROR_BILL_AMOUNT_MISSING");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_EXISTS");
            }

            if (partnerObj.status != 15)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_AVAIABLE");
            }

            if (partnerObj.bk_partner_code == null)
            {
                return new APIResponse("ERROR_PARTNER_BAOKIM_NOT_AVAIABLE");
            }
            var userPartner = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == request.partner_id).FirstOrDefault();

            if (userPartner == null)
            {
                return new APIResponse("ERROR_USER_PARTNER_NOT_EXISTS");
            }

            var contractObj = _context.PartnerContracts.Where(x => x.status == 12 && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id).FirstOrDefault();
            if (contractObj == null)
            {
                return new APIResponse("ERROR_CONTRACT_NOT_EXISTS");
            }
            var notiConfig = _context.NotiConfigs.FirstOrDefault();
            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null || settingObj.point_use == null || settingObj.point_save == null)
            {
                return new APIResponse("ERROR_SETTINGS_NOT_CONFIG");
            }

            var customerObj = _context.Customers.Where(x => x.id == request.customer_id).FirstOrDefault();
            if (customerObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_NOT_EXISTS");
            }

            CustomerBankAccount customerBankAccountObj = new CustomerBankAccount();

            customerBankAccountObj = _context.CustomerBankAccounts.Where(x => x.user_id == customerObj.id && x.is_default == true).FirstOrDefault();
            if (customerBankAccountObj == null)
            {
                customerBankAccountObj = _context.CustomerBankAccounts.Where(x => x.user_id == customerObj.id).FirstOrDefault();
                if (customerBankAccountObj == null)
                {
                    return new APIResponse("ERROR_CUSTOMER_BANK_ACCOUNT_NOT_EXISTS");
                }
            }

            var customerBankObj = _context.Banks.Where(x => x.id == customerBankAccountObj.bank_id).FirstOrDefault();
            if (customerBankObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_BANK_NOT_EXISTS");
            }

            var sysBankObj = _context.Banks.Where(x => x.id == settingObj.sys_receive_bank_id).FirstOrDefault();
            if (sysBankObj == null)
            {
                return new APIResponse("ERROR_SYSTEM_BANK_NOT_EXISTS");
            }

            var userCustomer = _context.Users.Where(x => x.is_customer == true && x.customer_id == request.customer_id).FirstOrDefault();
            if (userCustomer == null)
            {
                return new APIResponse("ERROR_USER_CUSTOMER_NOT_EXISTS");
            }

            decimal pointExchangeRate = Math.Round(((decimal)settingObj.point_exchange / (decimal)settingObj.point_value), 2);
            decimal customerExchange = 0;
            decimal affiliateExchange = 0;
            decimal systemExchange = 0;
            // Lấy cấu hình đổi điểm hiệu lực
            var accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id && x.status == 23).FirstOrDefault();

            // Nếu không có riêng thì lấy chung
            if (accumulateConfig == null)
            {
                accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();
            }

            if (accumulateConfig == null)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfig.id).ToList();

            if (listAccuDetails.Count == 0)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            for (int i = 0; i < listAccuDetails.Count; i++)
            {
                if (listAccuDetails[i].allocation_name == "Khách hàng")
                {
                    customerExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Điểm tích lũy")
                {
                    affiliateExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Hệ thống")
                {
                    systemExchange = (decimal)listAccuDetails[i].discount_rate;
                }
            }

            decimal total_point = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate * pointExchangeRate / 100);

            bool isDebit = false;

            decimal pointCustomer = Math.Round(total_point * customerExchange / 100);
            decimal pointAffiliate = Math.Round(total_point * affiliateExchange / 100);
            decimal pointSystem = Math.Round(total_point * systemExchange / 100);

            var total_scan_qr = _context.AccumulatePointOrders.Where(x => x.partner_id == request.partner_id && x.customer_id == request.customer_id).Count();

            var min_exchange_transaction = settingObj.cash_condition_value;
            var total_allow_cash = settingObj.total_allow_cash;

            var total_accumulate = _context.AccumulatePointOrders.Where(x => x.customer_id == request.customer_id && x.approve_date != null).Count();

            long amount_balance = 0;

            if (partnerObj.bk_partner_code != null)
            {
                GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

                amount_balance = balanceObj.Available;
            }

            decimal total_money = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate / 100);


            if (amount_balance < total_money)
            {
                request.status = 6;
                _context.SaveChanges();
                string mess = notiConfig.MC_Payment_CheckSurplus.Replace("{SoDu}", amount_balance.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{TongChietKhau}", total_money.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{CustomerName}", customerObj.full_name);
                return new APIResponse(mess);
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Nếu không đủ điều kiện hoàn tiền thì cộng điểm
                if (total_accumulate > total_allow_cash && ((pointCustomer * pointExchangeRate) < min_exchange_transaction))
                {
                    request.return_type = "Point";
                   
                    decimal amount =  (total_point * pointExchangeRate) - (decimal)settingObj.expense_fee;


                    //Thanh toán tiền mặt/Chuyển 100% từ deposit merchant sang CashPlus
                    //TransferResponseObj response = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", (total_point * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                    TransferResponseObj response = _bkTransaction.transferMoney(partnerObj.bk_partner_code, sysBankObj.bank_code, settingObj.sys_receive_bank_no, settingObj.sys_receive_bank_owner, amount, "Chuyen tien tai BK voi ma don hang " + request.trans_no, partnerObj.RSA_privateKey);

                    long amount_balance_new = 0;

                    if (partnerObj.bk_partner_code != null)
                    {
                        GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

                        amount_balance_new = balanceObj.Available;
                    }
                    // Log giao dịch chuyển cho NTD
                    BaoKimTransaction ntdTrans = new BaoKimTransaction();
                    ntdTrans.id = Guid.NewGuid();
                    ntdTrans.payment_type = "MER_TRANSFER_SYS";
                    ntdTrans.bao_kim_transaction_id = response.TransactionId;
                    ntdTrans.transaction_no = response.ReferenceId;
                    ntdTrans.amount = amount;
                    ntdTrans.accumulate_point_order_id = request.id;
                    ntdTrans.partner_id = request.partner_id;
                    ntdTrans.customer_id = request.customer_id;
                    ntdTrans.amount_balance = amount_balance_new;
                    ntdTrans.bank_receive_name = sysBankObj.name;
                    ntdTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                    ntdTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                    ntdTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                    if (response.ResponseCode == 200)
                    {
                        ntdTrans.trans_status = 25;
                    }
                    else
                    {
                        ntdTrans.trans_status = 26;
                    }
                    ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                    ntdTrans.transaction_date = DateTime.Now;

                    _context.BaoKimTransactions.Add(ntdTrans);
                    _context.SaveChanges();

                    // Cộng điểm khách hàng
                    var newCustomerPointHistory = new CustomerPointHistory();
                    newCustomerPointHistory.id = Guid.NewGuid();
                    newCustomerPointHistory.order_type = "PUSH";
                    newCustomerPointHistory.customer_id = request.customer_id;
                    //newCustomerPointHistory.source_id = request.customer_id;
                    newCustomerPointHistory.point_amount = pointCustomer;
                    newCustomerPointHistory.status = 4;
                    newCustomerPointHistory.trans_date = DateTime.Now;
                    userCustomer.point_avaiable += pointCustomer;
                    newCustomerPointHistory.point_type = "AVAIABLE";
                    newCustomerPointHistory.check_sms = false;
                    newCustomerPointHistory.send_sms = false;
                    newCustomerPointHistory.point_sms_total = 0;
                    _context.CustomerPointHistorys.Add(newCustomerPointHistory);
                    _context.SaveChanges();

                    userCustomer.total_point = userCustomer.point_affiliate + userCustomer.point_avaiable + userCustomer.point_waiting;
                    _common.checkUpdateCustomerRank((Guid)userCustomer.id);
                    //Gửi sms cho NTD nếu đủ hạn mức
                    //Tính tổng điểm tiêu dùng chưa gửi sms
                    var listCustomerPoints = _context.CustomerPointHistorys.Where(e => e.customer_id == request.customer_id
                    && e.point_type == "AVAIABLE" && e.check_sms != true).ToList();
                    var total_point_notsms = listCustomerPoints.Sum(e => e.point_amount).HasValue ? listCustomerPoints.Sum(e => e.point_amount).Value : 0;
                    var point_use = settingObj.point_use != null ? settingObj.point_use : 0;
                    var MC_Payment_SMSPointUse = notiConfig.Payment_SMSPointUse;
                    var customer = _context.Customers.Where(l => l.id == userCustomer.customer_id).FirstOrDefault();
                    if (total_point_notsms >= point_use && userCustomer.SMS_addPointUse == true)
                    {
                        var point = (decimal)userCustomer.point_avaiable;
                        _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, null, pointCustomer, customer.phone, (decimal)point_use, MC_Payment_SMSPointUse, null, point));

                        //Đánh dấu lại đã gửi
                        listCustomerPoints.ForEach(e => e.check_sms = true);
                        //Đánh dấu tại thời điểm gửi
                        newCustomerPointHistory.send_sms = true;
                        newCustomerPointHistory.point_sms_total = total_point_notsms;
                        _context.SaveChanges();
                    }
                }
                else
                {
                    // Đủ diều kiện hoàn tiền
                    request.return_type = "Cash";
                    decimal amount =  (pointCustomer * pointExchangeRate);
                    //Giao dịch tiền mặt
                    // Chuyển khoản từ deposit merchant sang NTD CK Merchant
                    //TransferResponseObj response = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", (pointCustomer * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                    TransferResponseObj response = _bkTransaction.transferMoney(partnerObj.bk_partner_code, customerBankObj.bank_code, customerBankAccountObj.bank_no, customerBankAccountObj.bank_owner, amount, "CashPlus hoan tien theo ma giao dich: " + request.trans_no, partnerObj.RSA_privateKey);
                    long amount_balance_new = 0;

                    if (partnerObj.bk_partner_code != null)
                    {
                        GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

                        amount_balance_new = balanceObj.Available;
                    }
                    // Log giao dịch chuyển cho NTD
                    BaoKimTransaction ntdTrans = new BaoKimTransaction();
                    ntdTrans.id = Guid.NewGuid();
                    ntdTrans.payment_type = "MER_TRANSFER_NTD";
                    ntdTrans.bao_kim_transaction_id = response.TransactionId;
                    ntdTrans.transaction_no = response.ReferenceId;
                    ntdTrans.amount = amount;
                    ntdTrans.accumulate_point_order_id = request.id;
                    ntdTrans.partner_id = request.partner_id;
                    ntdTrans.customer_id = request.customer_id;
                    ntdTrans.amount_balance =amount_balance_new;
                    ntdTrans.bank_receive_name = customerBankObj.name;
                    ntdTrans.bank_receive_account = customerBankAccountObj.bank_no;
                    ntdTrans.bank_receive_owner = customerBankAccountObj.bank_owner;
                    ntdTrans.transaction_description = "CashPlus hoan tien theo ma giao dich: " + request.trans_no;

                    if (response.ResponseCode == 200)
                    {
                        ntdTrans.trans_status = 25;
                    }
                    else
                    {
                        ntdTrans.trans_status = 26;
                    }
                    ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                    ntdTrans.transaction_date = DateTime.Now;

                    _context.BaoKimTransactions.Add(ntdTrans);
                    _context.SaveChanges();

                    decimal amount_  = ((pointSystem + pointAffiliate) * pointExchangeRate) - (decimal)(settingObj.expense_fee * 2);

                    // Chuyển khoản từ deposit merchant sang CashPlus CK CashPLus + Affiliate
                    //TransferResponseObj response2 = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", ((pointSystem + pointAffiliate) * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                    TransferResponseObj response2 = _bkTransaction.transferMoney(partnerObj.bk_partner_code, sysBankObj.bank_code, settingObj.sys_receive_bank_no, settingObj.sys_receive_bank_owner,amount_ , "Chuyen tien tai BK voi ma don hang: " + request.trans_no, partnerObj.RSA_privateKey);
                    if (partnerObj.bk_partner_code != null)
                    {
                        GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

                        amount_balance_new = balanceObj.Available;
                    }
                    // Log giao dịch chuyển sang cho CashPlus
                    BaoKimTransaction sysTrans = new BaoKimTransaction();
                    sysTrans.id = Guid.NewGuid();
                    sysTrans.payment_type = "MER_TRANSFER_SYS";
                    sysTrans.bao_kim_transaction_id = response2.TransactionId;
                    sysTrans.transaction_no = response2.ReferenceId;
                    sysTrans.amount = amount_;
                    sysTrans.accumulate_point_order_id = request.id;
                    sysTrans.partner_id = request.partner_id;
                    sysTrans.customer_id = request.customer_id;
                    sysTrans.transaction_description = "Chuyen tien tai BK voi ma don hang: " + request.trans_no;
                    sysTrans.amount_balance = amount_balance_new;
                    sysTrans.bank_receive_name = sysBankObj.name;
                    sysTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                    sysTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                    if (response2.ResponseCode == 200)
                    {
                        sysTrans.trans_status = 25;
                    }
                    else
                    {
                        sysTrans.trans_status = 26;
                    }
                    sysTrans.transaction_date = DateTime.Now;
                    sysTrans.trans_log = JsonSerializer.Serialize(response2, option);

                    _context.BaoKimTransactions.Add(sysTrans);
                    _context.SaveChanges();
                }

                _context.SaveChanges();

                // Xử lý điểm Affiliate
                decimal pointSystemAffiliate = pointAffiliate;
                string usernameLv1 = "";
                if (pointSystemAffiliate >= 0 && userCustomer.share_person_id != null)
                {
                    var userLv1 = _context.Users.Where(x => x.id == userCustomer.share_person_id).FirstOrDefault();

                    if (userLv1 != null && userLv1.is_delete != true && userLv1.status == 1)
                    {
                        usernameLv1 = userLv1.username;
                        userLv1.point_affiliate += pointSystemAffiliate;
                        userLv1.total_point = userLv1.point_affiliate + userLv1.point_avaiable + userLv1.point_waiting;
                        var AffiliateConfigs = _context.AffiliateConfigs.Where(p => p.code == "GENERAL").FirstOrDefault();
                        var date_return = AffiliateConfigs.date_return;
                        var time_return = AffiliateConfigs.hours_return;
                        if (userLv1.is_customer == true)
                        {
                            var newCustomerPointHistoryLv1 = new CustomerPointHistory();
                            newCustomerPointHistoryLv1.id = Guid.NewGuid();
                            newCustomerPointHistoryLv1.order_type = "AFF_LV_1";
                            newCustomerPointHistoryLv1.customer_id = userLv1.customer_id;
                            newCustomerPointHistoryLv1.source_id = request.customer_id;
                            newCustomerPointHistoryLv1.point_amount = pointSystemAffiliate;
                            newCustomerPointHistoryLv1.status = 4;
                            newCustomerPointHistoryLv1.trans_date = DateTime.Now;
                            newCustomerPointHistoryLv1.point_type = "WAITING";
                            newCustomerPointHistoryLv1.check_sms = false;
                            newCustomerPointHistoryLv1.send_sms = false;
                            newCustomerPointHistoryLv1.point_sms_total = 0;
                            _context.CustomerPointHistorys.Add(newCustomerPointHistoryLv1);
                            _context.SaveChanges(); 
                            //
                            //Gửi sms cho NGT nếu đủ hạn mức
                            //Tính tổng điểm tích lũy chưa gửi sms
                            var listCustomerPoints = _context.CustomerPointHistorys.Where(e => e.customer_id == userLv1.customer_id
                            && e.point_type == "WAITING" && e.check_sms != true).ToList();
                            var total_point_notsms = listCustomerPoints.Sum(e => e.point_amount).HasValue ? listCustomerPoints.Sum(e => e.point_amount).Value : 0;
                            var point_save = settingObj.point_save != null ? settingObj.point_save : 0;
                            var MC_Payment_SMSPointSave = notiConfig.Payment_SMSPointSave;
                            var send_time = settingObj.send_time != null ? settingObj.send_time : 0;
                            var userSendtime = userLv1.countSendSMS != null ? userLv1.countSendSMS : 0;
                            var check = false;
                            var customer = _context.Customers.Where(l => l.id == userLv1.customer_id).FirstOrDefault();
                            if (userSendtime < send_time && userLv1.SMS_addPointSave == true)
                            {
                                check = true;
                                var point = (decimal)userLv1.point_affiliate;
                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, customer.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                //Đánh dấu lại đã gửi
                                userLv1.countSendSMS = userSendtime + 1;
                                listCustomerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newCustomerPointHistoryLv1.send_sms = true;
                                newCustomerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }

                            if (total_point_notsms >= point_save && check == false && userLv1.SMS_addPointSave == true)
                            {
                                var point = (decimal)userLv1.point_affiliate;

                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, customer.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                userLv1.countSendSMS = userSendtime + 1;
                                //Đánh dấu lại đã gửi
                                listCustomerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newCustomerPointHistoryLv1.send_sms = true;
                                newCustomerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }
                        }
                        else if (userLv1.is_partner == true)
                        {
                            var newPartnerPointHistoryLv1 = new PartnerPointHistory();
                            newPartnerPointHistoryLv1.id = Guid.NewGuid();
                            newPartnerPointHistoryLv1.order_type = "AFF_LV_1";
                            newPartnerPointHistoryLv1.partner_id = userLv1.partner_id;
                            newPartnerPointHistoryLv1.source_id = request.customer_id;
                            newPartnerPointHistoryLv1.point_amount = pointSystemAffiliate;
                            newPartnerPointHistoryLv1.status = 4;
                            newPartnerPointHistoryLv1.trans_date = DateTime.Now;
                            newPartnerPointHistoryLv1.point_type = "WAITING";
                            newPartnerPointHistoryLv1.check_sms = false;
                            newPartnerPointHistoryLv1.send_sms = false;
                            newPartnerPointHistoryLv1.point_sms_total = 0;
                            _context.PartnerPointHistorys.Add(newPartnerPointHistoryLv1);
                            _context.SaveChanges();

                            //Gửi sms cho NGT nếu đủ hạn mức
                            //Tính tổng điểm tích lũy chưa gửi sms
                            var listPartnerPoints = _context.PartnerPointHistorys.Where(e => e.partner_id == userLv1.partner_id
                            && e.point_type == "WAITING" && e.check_sms != true).ToList();
                            var total_point_notsms = listPartnerPoints.Sum(e => e.point_amount).HasValue ? listPartnerPoints.Sum(e => e.point_amount).Value : 0;
                            var point_save = settingObj.point_save != null ? settingObj.point_save : 0;
                            var MC_Payment_SMSPointSave = notiConfig.Payment_SMSPointSave;
                            var send_time = settingObj.send_time != null ? settingObj.send_time : 0;
                            var userSendtime = userLv1.countSendSMS != null ? userLv1.countSendSMS : 0;
                            var check = false;
                            var Partner = _context.Partners.Where(l => l.id == userLv1.partner_id).FirstOrDefault();

                            if (userSendtime < send_time && userLv1.SMS_addPointSave == true)
                            {
                                check = true;
                                var point = (decimal)userLv1.point_affiliate;
                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, Partner.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                userLv1.countSendSMS = userSendtime + 1;

                                //Đánh dấu lại đã gửi
                                listPartnerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newPartnerPointHistoryLv1.send_sms = true;
                                newPartnerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }
                            if (total_point_notsms >= point_save && check == false && userLv1.SMS_addPointSave == true)
                            {
                                var point = (decimal)userLv1.point_affiliate;
                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, Partner.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                userLv1.countSendSMS = userSendtime + 1;

                                //Đánh dấu lại đã gửi
                                listPartnerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newPartnerPointHistoryLv1.send_sms = true;
                                newPartnerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }
                        }

                        _context.SaveChanges();

                        var newAffi = new AccumulatePointOrderAffiliate();
                        newAffi.id = Guid.NewGuid();
                        newAffi.accumulate_point_order_id = request.id;
                        newAffi.username = usernameLv1.Length > 0 ? usernameLv1 : "Hệ thống";
                        newAffi.discount_rate = affiliateExchange;
                        newAffi.date_created = DateTime.Now;
                        newAffi.levels = 1;
                        newAffi.point_value = pointSystemAffiliate;
                        _context.AccumulatePointOrderAffiliates.Add(newAffi);
                        _context.SaveChanges();

                        if (userLv1.device_id != null)
                        {
                            // Gửi FCM cho tài khoản phát triển cộng đồng
                            _fCMNotification.SendNotification(userLv1.device_id,
                            "WALLET",
                            "Điểm phát triển cộng đồng",
                            "Tài khoản của bạn đã nhận được " + pointAffiliate.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ việc phát triển cộng đồng",
                            null);

                            var newNoti1 = new Notification();
                            newNoti1.id = Guid.NewGuid();
                            newNoti1.title = "Điểm phát triển cộng đồng";
                            newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                            newNoti1.user_id = userLv1.is_partner == true ? userLv1.partner_id : userLv1.customer_id;
                            newNoti1.date_created = DateTime.Now;
                            newNoti1.date_updated = DateTime.Now;
                            newNoti1.content = "Tài khoản của bạn đã nhận được " + pointAffiliate.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ việc phát triển cộng đồng";
                            newNoti1.system_type = "WALLET";
                            newNoti1.reference_id = null;

                            _context.Notifications.Add(newNoti1);
                            _context.SaveChanges();
                        }
                    }
                }

                // Cộng điểm hệ thống
                var newSystemPointHistory = new SystemPointHistory();
                newSystemPointHistory.id = Guid.NewGuid();
                newSystemPointHistory.order_type = "PUSH";
                newSystemPointHistory.point_amount = pointSystem;
                newSystemPointHistory.status = 4;
                newSystemPointHistory.trans_date = DateTime.Now;

                if (isDebit == true)
                {
                    newSystemPointHistory.point_type = "WAITING";
                }
                else
                {
                    newSystemPointHistory.point_type = "AVAIABLE";
                }

                _context.SystemPointHistorys.Add(newSystemPointHistory);

                _context.SaveChanges();


                // Lưu thông tin điểm hóa đơn
                request.status = 5;
                request.discount_rate = contractObj.discount_rate;
                request.point_exchange = total_point;
                if (isDebit == true)
                {
                    request.point_waiting = total_point;
                    request.point_avaiable = 0;
                }
                else
                {
                    request.point_avaiable = total_point;
                    request.point_waiting = 0;
                }
                //request.point_partner = total_point;
                request.point_customer = pointCustomer;
                request.point_system = pointSystem;
                request.approve_user = username;
                request.user_updated = username;
                request.approve_date = DateTime.Now;
                request.date_updated = DateTime.Now;
                _context.SaveChanges();

                // Tạo detail
                for (int i = 0; i < listAccuDetails.Count; i++)
                {
                    var newDetail = new AccumulatePointOrderDetail();
                    newDetail.id = Guid.NewGuid();
                    newDetail.accumulate_point_order_id = request.id;
                    newDetail.name = listAccuDetails[i].name;
                    newDetail.discount_rate = listAccuDetails[i].discount_rate;
                    if (listAccuDetails[i].name == "Khách hàng")
                    {
                        newDetail.point_value = pointCustomer;
                        newDetail.allocation_name = "Người tiêu dùng";
                        newDetail.description = "Chiết khấu cho NTD";
                    }
                    else if (listAccuDetails[i].name == "Hệ thống")
                    {
                        newDetail.point_value = pointSystem;
                        newDetail.allocation_name = "Hệ thống(Admin)";
                        newDetail.description = "Chiết khấu cho CashPlus";
                    }
                    else if (listAccuDetails[i].name == "Điểm tích lũy")
                    {
                        newDetail.point_value = pointAffiliate;
                        newDetail.allocation_name = "Điểm tích lũy";
                        newDetail.description = "Chiết khấu cho người giới thiệu sẽ được chuyển về CashPlus, từ đó CashPlus sẽ cộng điểm tích lũy cho NGT";
                    }

                    _context.AccumulatePointOrderDetails.Add(newDetail);
                }

                _context.SaveChanges();

                // Gửi FCM
                // Cho khách
                if (userCustomer.device_id != null)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userCustomer.customer_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    if (request.return_type == "Cash")
                    {
                        newNoti1.title = "Hoàn tiền tiêu dùng";
                        //newNoti1.content = "Tài khoản của bạn vừa nhận được " + (pointCustomer * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " VNĐ từ giao dịch " + request.trans_no + " với số tiền thanh toán là: " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " vào lúc " + _commonFunction.convertDateToStringFull(request.date_created);
                        var notiReturn = notiConfig.Payment_Refund;
                        newNoti1.content = notiReturn.Replace("{TienHoan}", (pointCustomer * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{MaGiaoDich}", request.trans_no).Replace("{SoTienGiaoDich}", ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(request.date_created));
                    }
                    else
                    {
                        newNoti1.title = "Tích điểm tiêu dùng";
                        var notiReturn = notiConfig.Payment_NotEnRefund;

                        //newNoti1.content = "Tài khoản của bạn vừa nhận được " + pointCustomer.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ giao dịch " + request.trans_no + " với số tiền thanh toán là: " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " vào lúc " + _commonFunction.convertDateToStringFull(request.date_created);
                        newNoti1.content = notiReturn.Replace("{DiemTD}", pointCustomer.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{MaGiaoDich}", request.trans_no).Replace("{SoTienGiaoDich}", ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(request.date_created));

                    }
                    newNoti1.system_type = "ACCU_POINT";
                    newNoti1.reference_id = request.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    if (userCustomer.send_Notification == true)
                    {
                        _fCMNotification.SendNotification(userCustomer.device_id,
                           "ACCU_POINT",
                           newNoti1.title,
                           newNoti1.content,
                           request.id);
                    }
                }

                // Cho shop
                if (userPartner.device_id != null)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Hoàn tiền tiêu dùng";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userPartner.partner_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    //newNoti1.content = "Tài khoản của bạn đã bị trừ " + (total_point * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + "VNĐ từ giao dịch " + request.trans_no + " xác nhậm thanh toán số tiền " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." });
                    var returnNoti = notiConfig.MC_Payment_Refund;
                    newNoti1.content = returnNoti.Replace("{TongTienChietKhau}", (total_point * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{MaGiaoDich}", request.trans_no).Replace("{SoTienGiaoDich}", ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }));

                    newNoti1.system_type = "ACCU_POINT";
                    newNoti1.reference_id = request.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    if (userPartner.send_Notification == true)
                    {
                        _fCMNotification.SendNotification(userPartner.device_id,
                           "ACCU_POINT",
                           newNoti1.title,
                           newNoti1.content,
                           request.id);
                    }
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(ex.Message);
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(new
            {
                total_scan_qr = total_scan_qr != null ? (total_scan_qr + 1) : 1
            });
        }

        //tạo link VietQR BK
        public async Task<APIResponse> createPaymentLink(AccumulatePointOrderRequest request)
        {
            if (request.order_id == null)
            {
                return new APIResponse("ERROR_ORDER_ID_MISSING");
            }

            var orderObj = _context.AccumulatePointOrders.Where(x => x.id == request.order_id).FirstOrDefault();

            string return_url = "";
            if (orderObj != null)
            {
                return_url = await _bkTransaction.createPaymentLink((Guid)request.order_id);
                orderObj.payment_type = "BaoKim";
                _context.SaveChanges();


            }
            var return_url_split = return_url.Split("+/");
            var check = return_url_split[0];

            int status = 200;
            if (check.Contains("Error")) status = 500;

            return new APIResponse(status, new { payment_link = return_url_split[1] });
        }

        public async Task<APIResponse> createPaymentLinkFull(AccumulatePointOrderRequest request)
        {
            if (request.order_id == null)
            {
                return new APIResponse("ERROR_ORDER_ID_MISSING");
            }

            var orderObj = _context.AccumulatePointOrders.Where(x => x.id == request.order_id).FirstOrDefault();

            string return_url = "";
            if (orderObj != null)
            {
                return_url = await _bkTransaction.createPaymentLinkFull((Guid)request.order_id);
                orderObj.payment_type = "BaoKim";
                _context.SaveChanges();
            }
            var return_url_split = return_url.Split("+/");
            var check = return_url_split[0];

            int status = 200;
            if (check.Contains("Error")) status = 500;

            return new APIResponse(status, new { payment_link = return_url_split[1] });
        }
        //Thanh toán online
        public APIResponse cashPaymentOnline(Guid id, string username)
        {

            var request = _context.AccumulatePointOrders.Where(x => x.id == id).FirstOrDefault();

            if (request == null)
            {
                return new APIResponse("ERROR_ORDER_ID_NOT_EXISTS");
            }

            var dateNow = DateTime.Now;

            if (request.date_created.Value.AddMinutes(20) <= dateNow)
            {
                return new APIResponse("ERROR_ORDER_EXPIRE");
            }

            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.bill_amount == null)
            {
                return new APIResponse("ERROR_BILL_AMOUNT_MISSING");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_EXISTS");
            }

            if (partnerObj.status != 15)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_AVAIABLE");
            }

            if (partnerObj.bk_partner_code == null)
            {
                return new APIResponse("ERROR_PARTNER_BAOKIM_NOT_AVAIABLE");
            }

            var userPartner = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == request.partner_id).FirstOrDefault();

            if (userPartner == null)
            {
                return new APIResponse("ERROR_USER_PARTNER_NOT_EXISTS");
            }

            var contractObj = _context.PartnerContracts.Where(x => x.status == 12 && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id).FirstOrDefault();
            if (contractObj == null)
            {
                return new APIResponse("ERROR_CONTRACT_NOT_EXISTS");
            }

            var notiConfig = _context.NotiConfigs.FirstOrDefault();
            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null || settingObj.point_use == null || settingObj.point_save == null)
            {
                return new APIResponse("ERROR_SETTINGS_NOT_CONFIG");
            }

            var customerObj = _context.Customers.Where(x => x.id == request.customer_id).FirstOrDefault();
            if (customerObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_NOT_EXISTS");
            }

            CustomerBankAccount customerBankAccountObj = new CustomerBankAccount();

            customerBankAccountObj = _context.CustomerBankAccounts.Where(x => x.user_id == customerObj.id && x.is_default == true).FirstOrDefault();
            if (customerBankAccountObj == null)
            {
                customerBankAccountObj = _context.CustomerBankAccounts.Where(x => x.user_id == customerObj.id).FirstOrDefault();
                if (customerBankAccountObj == null)
                {
                    return new APIResponse("ERROR_CUSTOMER_BANK_ACCOUNT_NOT_EXISTS");
                }
            }

            var customerBankObj = _context.Banks.Where(x => x.id == customerBankAccountObj.bank_id).FirstOrDefault();
            if (customerBankObj == null)
            {
                return new APIResponse("ERROR_CUSTOMER_BANK_NOT_EXISTS");
            }

            var sysBankObj = _context.Banks.Where(x => x.id == settingObj.sys_receive_bank_id).FirstOrDefault();
            if (sysBankObj == null)
            {
                return new APIResponse("ERROR_SYSTEM_BANK_NOT_EXISTS");
            }

            var userCustomer = _context.Users.Where(x => x.is_customer == true && x.customer_id == request.customer_id).FirstOrDefault();
            if (userCustomer == null)
            {
                return new APIResponse("ERROR_USER_CUSTOMER_NOT_EXISTS");
            }

            decimal pointExchangeRate = Math.Round(((decimal)settingObj.point_exchange / (decimal)settingObj.point_value), 2);
            decimal customerExchange = 0;
            decimal affiliateExchange = 0;
            decimal systemExchange = 0;
            // Lấy cấu hình đổi điểm hiệu lực
            var accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id && x.status == 23).FirstOrDefault();

            // Nếu không có riêng thì lấy chung
            if (accumulateConfig == null)
            {
                accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();
            }

            if (accumulateConfig == null)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfig.id).ToList();

            if (listAccuDetails.Count == 0)
            {
                return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
            }

            for (int i = 0; i < listAccuDetails.Count; i++)
            {
                if (listAccuDetails[i].allocation_name == "Khách hàng")
                {
                    customerExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Điểm tích lũy")
                {
                    affiliateExchange = (decimal)listAccuDetails[i].discount_rate;
                }
                else if (listAccuDetails[i].allocation_name == "Hệ thống")
                {
                    systemExchange = (decimal)listAccuDetails[i].discount_rate;
                }
            }
            //Tổng thanh toán
            decimal total_point = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate * pointExchangeRate / 100);

            bool isDebit = false;
            //Tính điểm 
            decimal pointCustomer = Math.Round(total_point * customerExchange / 100);
            decimal pointAffiliate = Math.Round(total_point * affiliateExchange / 100);
            decimal pointSystem = Math.Round(total_point * systemExchange / 100);

            var total_scan_qr = _context.AccumulatePointOrders.Where(x => x.partner_id == request.partner_id && x.customer_id == request.customer_id).Count();

            var min_exchange_transaction = settingObj.cash_condition_value;
            var total_allow_cash = settingObj.total_allow_cash;

            var total_accumulate = _context.AccumulatePointOrders.Where(x => x.customer_id == request.customer_id && x.approve_date != null).Count();

            // long amount_balance = 0;
            //if (partnerObj.bk_partner_code != null)
            //{
            //    //Kiểm tra số dư
            //    GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

            //    amount_balance = balanceObj.Available;
            //}

            decimal total_money = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate / 100);


            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Nếu không đủ điều kiện hoàn tiền thì cộng điểm
                if (total_accumulate > total_allow_cash && ((pointCustomer * pointExchangeRate) < min_exchange_transaction))
                {
                    request.return_type = "Point";
                    // Nếu thanh toán tiền mặt/Chuyển 100% từ deposit merchant sang CashPlus
                    //if (request.payment_type == "Cash")
                    //{
                    //    if (amount_balance < total_money)
                    //    {
                    //        throw new Exception("ERROR_PARTNER_ENOUGH_AMOUNT");
                    //    }

                    //    //TransferResponseObj response = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", (total_point * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                    //    TransferResponseObj response = _bkTransaction.transferMoney(partnerObj.bk_partner_code, sysBankObj.bank_code, settingObj.sys_receive_bank_no, settingObj.sys_receive_bank_owner, (total_point * pointExchangeRate), "Chuyen tien tai BK voi ma don hang " + request.trans_no, partnerObj.RSA_privateKey);

                    //    // Log giao dịch chuyển cho NTD
                    //    BaoKimTransaction ntdTrans = new BaoKimTransaction();
                    //    ntdTrans.id = Guid.NewGuid();
                    //    ntdTrans.payment_type = "MER_TRANSFER_SYS";
                    //    ntdTrans.bao_kim_transaction_id = response.TransactionId;
                    //    ntdTrans.transaction_no = response.ReferenceId;
                    //    ntdTrans.amount = (total_point * pointExchangeRate);
                    //    ntdTrans.accumulate_point_order_id = request.id;
                    //    ntdTrans.partner_id = request.partner_id;
                    //    ntdTrans.customer_id = request.customer_id;

                    //    ntdTrans.bank_receive_name = sysBankObj.name;
                    //    ntdTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                    //    ntdTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                    //    ntdTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                    //    if (response.ResponseCode == 200)
                    //    {
                    //        ntdTrans.trans_status = 25;
                    //    }
                    //    else
                    //    {
                    //        ntdTrans.trans_status = 26;
                    //    }
                    //    ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                    //    ntdTrans.transaction_date = DateTime.Now;

                    //    _context.BaoKimTransactions.Add(ntdTrans);
                    //    _context.SaveChanges();
                    //}
                    //else
                    //{ 
                    // Nếu thanh toán online thì tạo lệnh chờ giao dịch\

                    // Log giao dịch chuyển cho Hệ thống (khóa tạm)
                    BaoKimTransaction sysTrans = new BaoKimTransaction();
                    sysTrans.id = Guid.NewGuid();
                    sysTrans.payment_type = "MER_TRANSFER_SYS";
                    sysTrans.bao_kim_transaction_id = "";
                    sysTrans.transaction_no = request.trans_no;
                    sysTrans.amount = (total_point * pointExchangeRate);
                    sysTrans.accumulate_point_order_id = request.id;
                    sysTrans.partner_id = request.partner_id;
                    sysTrans.customer_id = request.customer_id;

                    sysTrans.bank_receive_name = sysBankObj.name;
                    sysTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                    sysTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                    sysTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                    sysTrans.trans_status = 25;
                    //ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                    sysTrans.transaction_date = DateTime.Now;

                    _context.BaoKimTransactions.Add(sysTrans);
                    _context.SaveChanges();
                    //}

                    // Cộng điểm khách hàng
                    var newCustomerPointHistory = new CustomerPointHistory();
                    newCustomerPointHistory.id = Guid.NewGuid();
                    newCustomerPointHistory.order_type = "PUSH";
                    newCustomerPointHistory.customer_id = request.customer_id;
                    //newCustomerPointHistory.source_id = request.customer_id;
                    newCustomerPointHistory.point_amount = pointCustomer;
                    newCustomerPointHistory.status = 4;
                    newCustomerPointHistory.trans_date = DateTime.Now;
                    userCustomer.point_avaiable += pointCustomer;
                    newCustomerPointHistory.point_type = "AVAIABLE";
                    newCustomerPointHistory.check_sms = false;
                    newCustomerPointHistory.send_sms = false;
                    newCustomerPointHistory.point_sms_total = 0;
                    _context.CustomerPointHistorys.Add(newCustomerPointHistory);

                    userCustomer.total_point = userCustomer.point_affiliate + userCustomer.point_avaiable + userCustomer.point_waiting;
                    _context.SaveChanges();
                    _common.checkUpdateCustomerRank((Guid)userCustomer.id);

                    //Gửi sms cho NTD nếu đủ hạn mức
                    //Tính tổng điểm tiêu dùng chưa gửi sms
                    var listCustomerPoints = _context.CustomerPointHistorys.Where(e => e.customer_id == request.customer_id
                    && e.point_type == "AVAIABLE" && e.check_sms != true).ToList();
                    var total_point_notsms = listCustomerPoints.Sum(e => e.point_amount).HasValue ? listCustomerPoints.Sum(e => e.point_amount).Value : 0;
                    var point_use = settingObj.point_use != null ? settingObj.point_use : 0;
                    var DataUser = _context.Users.Where(l => l.customer_id == userCustomer.customer_id).FirstOrDefault();
                    var customer = _context.Customers.Where(l => l.id == userCustomer.customer_id).FirstOrDefault();
                    var MC_Payment_SMSPointUse = notiConfig.MC_Payment_SMSPointUse;
                    if (total_point_notsms >= point_use && userCustomer.SMS_addPointUse == true)
                    {
                        var point = (decimal)userCustomer.point_avaiable;
                        _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, null, pointCustomer, customer.phone, (decimal)point_use, MC_Payment_SMSPointUse, null, point));

                        //Đánh dấu lại đã gửi
                        listCustomerPoints.ForEach(e => e.check_sms = true);
                        //Đánh dấu tại thời điểm gửi
                        newCustomerPointHistory.send_sms = true;
                        newCustomerPointHistory.point_sms_total = total_point_notsms;
                        _context.SaveChanges();
                    }
                }
                else
                {
                    // Đủ diều kiện hoàn tiền
                    request.return_type = "Cash";

                    //if (request.payment_type == "Cash")
                    //{ // Nếu giao dịch tiền mặt
                    //    if (amount_balance < total_money)
                    //    {
                    //        throw new Exception("ERROR_PARTNER_ENOUGH_AMOUNT");
                    //    }
                    //    // Chuyển khoản từ deposit merchant sang NTD CK Merchant
                    //    //TransferResponseObj response = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", (pointCustomer * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                    //    TransferResponseObj response = _bkTransaction.transferMoney(partnerObj.bk_partner_code, customerBankObj.bank_code, customerBankAccountObj.bank_no, customerBankAccountObj.bank_owner, (pointCustomer * pointExchangeRate), "Chuyen tien tai BK voi ma don hang " + request.trans_no, partnerObj.RSA_privateKey);

                    //    // Log giao dịch chuyển cho NTD
                    //    BaoKimTransaction ntdTrans = new BaoKimTransaction();
                    //    ntdTrans.id = Guid.NewGuid();
                    //    ntdTrans.payment_type = "MER_TRANSFER_NTD";
                    //    ntdTrans.bao_kim_transaction_id = response.TransactionId;
                    //    ntdTrans.transaction_no = response.ReferenceId;
                    //    ntdTrans.amount = (pointCustomer * pointExchangeRate);
                    //    ntdTrans.accumulate_point_order_id = request.id;
                    //    ntdTrans.partner_id = request.partner_id;
                    //    ntdTrans.customer_id = request.customer_id;

                    //    ntdTrans.bank_receive_name = customerBankObj.name;
                    //    ntdTrans.bank_receive_account = customerBankAccountObj.bank_no;
                    //    ntdTrans.bank_receive_owner = customerBankAccountObj.bank_owner;
                    //    ntdTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                    //    if (response.ResponseCode == 200)
                    //    {
                    //        ntdTrans.trans_status = 25;
                    //    }
                    //    else
                    //    {
                    //        ntdTrans.trans_status = 26;
                    //    }
                    //    ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                    //    ntdTrans.transaction_date = DateTime.Now;

                    //    _context.BaoKimTransactions.Add(ntdTrans);
                    //    _context.SaveChanges();

                    //    // Chuyển khoản từ deposit merchant sang CashPlus CK CashPLus + Affiliate
                    //    //TransferResponseObj response2 = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", ((pointSystem + pointAffiliate) * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                    //    TransferResponseObj response2 = _bkTransaction.transferMoney(partnerObj.bk_partner_code, sysBankObj.bank_code, settingObj.sys_receive_bank_no, settingObj.sys_receive_bank_owner, ((pointSystem + pointAffiliate) * pointExchangeRate), "Chuyen tien tai BK voi ma don hang " + request.trans_no, partnerObj.RSA_privateKey);

                    //    // Log giao dịch chuyển sang cho CashPlus
                    //    BaoKimTransaction sysTrans = new BaoKimTransaction();
                    //    sysTrans.id = Guid.NewGuid();
                    //    sysTrans.payment_type = "MER_TRANSFER_SYS";
                    //    sysTrans.bao_kim_transaction_id = response2.TransactionId;
                    //    sysTrans.transaction_no = response2.ReferenceId;
                    //    sysTrans.amount = ((pointAffiliate + pointSystem) * pointExchangeRate);
                    //    sysTrans.accumulate_point_order_id = request.id;
                    //    sysTrans.partner_id = request.partner_id;
                    //    sysTrans.customer_id = request.customer_id;
                    //    sysTrans.transaction_description = "Chuyen tien tai BK voi ma don hang " + request.trans_no;

                    //    sysTrans.bank_receive_name = sysBankObj.name;
                    //    sysTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                    //    sysTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                    //    if (response2.ResponseCode == 200)
                    //    {
                    //        sysTrans.trans_status = 25;
                    //    }
                    //    else
                    //    {
                    //        sysTrans.trans_status = 26;
                    //    }
                    //    sysTrans.transaction_date = DateTime.Now;
                    //    sysTrans.trans_log = JsonSerializer.Serialize(response2, option);

                    //    _context.BaoKimTransactions.Add(sysTrans);
                    //    _context.SaveChanges();
                    //}
                    //else
                    //{
                    // Nếu giao dịch online
                    // Log giao dịch cho NTD
                    // Chuyển tiền từ CashPlus cho NTD
                    //TransferResponseObj response = _bkTransaction.transferMoney(Consts.CP_BK_PARTNER_CODE, "970409", "9704060224009513", "Nguyen Van A", (pointCustomer * pointExchangeRate).ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);
                    // decimal amount =  (pointCustomer * pointExchangeRate) - (decimal)settingObj.collection_fee - (decimal)settingObj.expense_fee;;
                    // if(amount >  settingObj.amount_limit){
                    //     amount =  amount - (decimal)settingObj.expense_fee - (decimal)(settingObj.collection_fee * 2);
                    // }

                    var PartnersData = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();
                    
                    // TransferResponseObj response = _bkTransaction.transferMoney(Consts.CP_BK_PARTNER_CODE, customerBankObj.bank_code, customerBankAccountObj.bank_no, customerBankAccountObj.bank_owner,(pointCustomer * pointExchangeRate) , "CashPlus hoan tien theo ma giao dich: " + request.trans_no, Consts.private_key);
                    TransferResponseObj response = _bkTransaction.transferMoney(PartnersData.bk_partner_code, customerBankObj.bank_code, customerBankAccountObj.bank_no, customerBankAccountObj.bank_owner, (pointCustomer * pointExchangeRate), "CashPlus hoan tien theo ma giao dich: " + request.trans_no, PartnersData.RSA_privateKey);


                    // Log giao dịch chuyển cho NTD
                    BaoKimTransaction ntdTrans = new BaoKimTransaction();
                    ntdTrans.id = Guid.NewGuid();
                    // ntdTrans.payment_type = "CP_TRANSFER_NTD";
                    ntdTrans.payment_type = "MRC_TRANSFER_NTD";
                    ntdTrans.bao_kim_transaction_id = response.TransactionId;
                    ntdTrans.transaction_no = response.ReferenceId;
                    ntdTrans.amount = (pointCustomer * pointExchangeRate);
                    ntdTrans.accumulate_point_order_id = request.id;
                    ntdTrans.partner_id = request.partner_id;
                    ntdTrans.customer_id = request.customer_id;

                    ntdTrans.bank_receive_name = customerBankObj.name;
                    ntdTrans.bank_receive_account = customerBankAccountObj.bank_no;
                    ntdTrans.bank_receive_owner = customerBankAccountObj.bank_owner;
                    ntdTrans.transaction_description = "CashPlus hoan tien theo ma giao dich: " + request.trans_no;

                    if (response.ResponseCode == 200)
                    {
                        ntdTrans.trans_status = 25;
                    }
                    else
                    {
                        ntdTrans.trans_status = 26;
                    }
                    ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                    ntdTrans.transaction_date = DateTime.Now;

                    _context.BaoKimTransactions.Add(ntdTrans);
                    _context.SaveChanges();

                    // Log giao dịch chuyển cho Hệ thống
                    BaoKimTransaction sysTrans = new BaoKimTransaction();
                    sysTrans.id = Guid.NewGuid();
                    sysTrans.payment_type = "MER_TRANSFER_SYS";
                    sysTrans.bao_kim_transaction_id = "";
                    sysTrans.transaction_no = request.trans_no;
                    // sysTrans.amount = ((pointSystem + pointAffiliate) * pointExchangeRate);
                    sysTrans.amount = pointSystem + pointAffiliate ;
                    sysTrans.accumulate_point_order_id = request.id;
                    sysTrans.partner_id = request.partner_id;
                    sysTrans.customer_id = request.customer_id;
                    sysTrans.bank_receive_name = sysBankObj.name;
                    sysTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                    sysTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                    sysTrans.transaction_description = "Chuyen tien tai BK CASHPLUS voi ma don hang " + request.trans_no;

                    sysTrans.trans_status = 25;
                    //ntdTrans.trans_log = JsonSerializer.Serialize(response, option);
                    ntdTrans.transaction_date = DateTime.Now;

                    _context.BaoKimTransactions.Add(sysTrans);
                    _context.SaveChanges();
                    //}
                }

                _context.SaveChanges();

                // Xử lý điểm Affiliate
                decimal pointSystemAffiliate = pointAffiliate;
                string usernameLv1 = "";
                if (pointSystemAffiliate >= 0 && userCustomer.share_person_id != null)
                {
                    var userLv1 = _context.Users.Where(x => x.id == userCustomer.share_person_id).FirstOrDefault();

                    if (userLv1 != null && userLv1.is_delete != true && userLv1.status == 1)
                    {
                        usernameLv1 = userLv1.username;
                        userLv1.point_affiliate += pointSystemAffiliate;
                        userLv1.total_point = userLv1.point_affiliate + userLv1.point_avaiable + userLv1.point_waiting;

                        var AffiliateConfigs = _context.AffiliateConfigs.Where(p => p.code == "GENERAL").FirstOrDefault();
                        var date_return = AffiliateConfigs.date_return;
                        var time_return = AffiliateConfigs.hours_return;
                        if (userLv1.is_customer == true)
                        {
                            var newCustomerPointHistoryLv1 = new CustomerPointHistory();
                            newCustomerPointHistoryLv1.id = Guid.NewGuid();
                            newCustomerPointHistoryLv1.order_type = "AFF_LV_1";
                            newCustomerPointHistoryLv1.customer_id = userLv1.customer_id;
                            newCustomerPointHistoryLv1.source_id = request.customer_id;
                            newCustomerPointHistoryLv1.point_amount = pointSystemAffiliate;
                            newCustomerPointHistoryLv1.status = 4;
                            newCustomerPointHistoryLv1.trans_date = DateTime.Now;
                            newCustomerPointHistoryLv1.point_type = "WAITING";
                            newCustomerPointHistoryLv1.check_sms = false;
                            newCustomerPointHistoryLv1.send_sms = false;
                            newCustomerPointHistoryLv1.point_sms_total = 0;
                            _context.CustomerPointHistorys.Add(newCustomerPointHistoryLv1);
                            _context.SaveChanges();
                            //
                            //Gửi sms cho NGT nếu đủ hạn mức
                            //Tính tổng điểm tích lũy chưa gửi sms
                            var listCustomerPoints = _context.CustomerPointHistorys.Where(e => e.customer_id == userLv1.customer_id
                            && e.point_type == "WAITING" && e.check_sms != true).ToList();
                            var total_point_notsms = listCustomerPoints.Sum(e => e.point_amount).HasValue ? listCustomerPoints.Sum(e => e.point_amount).Value : 0;
                            var point_save = settingObj.point_save != null ? settingObj.point_save : 0;
                            var MC_Payment_SMSPointSave = notiConfig.MC_Payment_SMSPointSave;
                            var send_time = settingObj.send_time != null ? settingObj.send_time : 0;
                            var userSendtime = userLv1.countSendSMS != null ? userLv1.countSendSMS : 0;
                            var check = false;
                            var customer = _context.Customers.Where(l => l.id == userLv1.customer_id).FirstOrDefault();

                            if (userSendtime < send_time && userLv1.SMS_addPointSave == true)
                            {
                                check = true;
                                var point = (decimal)userLv1.point_affiliate;

                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, customer.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                userLv1.countSendSMS = userSendtime + 1;
                                //Đánh dấu lại đã gửi
                                listCustomerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newCustomerPointHistoryLv1.send_sms = true;
                                newCustomerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }
                            if (total_point_notsms >= point_save && check == false && userLv1.SMS_addPointSave == true)
                            {
                                var point = (decimal)userLv1.point_affiliate;

                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, customer.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                userLv1.countSendSMS = userSendtime + 1;
                                //Đánh dấu lại đã gửi
                                listCustomerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newCustomerPointHistoryLv1.send_sms = true;
                                newCustomerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }
                        }
                        else if (userLv1.is_partner == true)
                        {
                            var newPartnerPointHistoryLv1 = new PartnerPointHistory();
                            newPartnerPointHistoryLv1.id = Guid.NewGuid();
                            newPartnerPointHistoryLv1.order_type = "AFF_LV_1";
                            newPartnerPointHistoryLv1.partner_id = userLv1.partner_id;
                            newPartnerPointHistoryLv1.source_id = request.customer_id;
                            newPartnerPointHistoryLv1.point_amount = pointSystemAffiliate;
                            newPartnerPointHistoryLv1.status = 4;
                            newPartnerPointHistoryLv1.trans_date = DateTime.Now;
                            newPartnerPointHistoryLv1.point_type = "WAITING";
                            newPartnerPointHistoryLv1.check_sms = false;
                            newPartnerPointHistoryLv1.send_sms = false;
                            newPartnerPointHistoryLv1.point_sms_total = 0;
                            _context.PartnerPointHistorys.Add(newPartnerPointHistoryLv1);
                            _context.SaveChanges();
                            //
                            //Gửi sms cho NGT nếu đủ hạn mức
                            //Tính tổng điểm tích lũy chưa gửi sms
                            var listPartnerPoints = _context.PartnerPointHistorys.Where(e => e.partner_id == userLv1.partner_id
                            && e.point_type == "WAITING" && e.check_sms != true).ToList();
                            var total_point_notsms = listPartnerPoints.Sum(e => e.point_amount).HasValue ? listPartnerPoints.Sum(e => e.point_amount).Value : 0;
                            var point_save = settingObj.point_save != null ? settingObj.point_save : 0;
                            var MC_Payment_SMSPointSave = notiConfig.MC_Payment_SMSPointSave;
                            var send_time = settingObj.send_time != null ? settingObj.send_time : 0;
                            var userSendtime = userLv1.countSendSMS != null ? userLv1.countSendSMS : 0;
                            var check = false;
                            var Partner = _context.Partners.Where(l => l.id == userLv1.partner_id).FirstOrDefault();

                            if (userSendtime < send_time && userLv1.SMS_addPointSave == true)
                            {
                                check = true;
                                var point = (decimal)userLv1.point_affiliate;
                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, Partner.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                userLv1.countSendSMS = userSendtime + 1;
                                //Đánh dấu lại đã gửi
                                listPartnerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newPartnerPointHistoryLv1.send_sms = true;
                                newPartnerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }
                            if (total_point_notsms >= point_save && check == false && userLv1.SMS_addPointSave == true)
                            {
                                var point = (decimal)userLv1.point_affiliate;
                                _ = Task.Run(() => _sendSMSBrandName.SendSMSBrandNameAsync(_serviceScopeFactory, 1, pointAffiliate, Partner.phone, (decimal)point_save, MC_Payment_SMSPointSave, date_return.ToString(), point));
                                userLv1.countSendSMS = userSendtime + 1;
                                //Đánh dấu lại đã gửi
                                listPartnerPoints.ForEach(e => e.check_sms = true);
                                //Đánh dấu tại thời điểm gửi
                                newPartnerPointHistoryLv1.send_sms = true;
                                newPartnerPointHistoryLv1.point_sms_total = total_point_notsms;
                                _context.SaveChanges();
                            }
                        }

                        var newAffi = new AccumulatePointOrderAffiliate();
                        newAffi.id = Guid.NewGuid();
                        newAffi.accumulate_point_order_id = request.id;
                        newAffi.username = usernameLv1.Length > 0 ? usernameLv1 : "Hệ thống";
                        newAffi.discount_rate = affiliateExchange;
                        newAffi.date_created = DateTime.Now;
                        newAffi.levels = 1;
                        newAffi.point_value = pointSystemAffiliate;

                        _context.AccumulatePointOrderAffiliates.Add(newAffi);
                        _context.SaveChanges();

                        if (userLv1.device_id != null)
                        {
                            if (userLv1.send_Notification == true)
                            {
                                // Gửi FCM cho tài khoản phát triển cộng đồng
                                _fCMNotification.SendNotification(userLv1.device_id,
                                "WALLET",
                                "Điểm phát triển cộng đồng",
                                "Tài khoản của bạn đã nhận được " + pointAffiliate.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ việc phát triển cộng đồng",
                                null);
                            }


                            var newNoti1 = new Notification();
                            newNoti1.id = Guid.NewGuid();
                            newNoti1.title = "Điểm phát triển cộng đồng";
                            newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                            newNoti1.user_id = userLv1.is_partner == true ? userLv1.partner_id : userLv1.customer_id;
                            newNoti1.date_created = DateTime.Now;
                            newNoti1.date_updated = DateTime.Now;
                            newNoti1.content = "Tài khoản của bạn đã nhận được " + pointAffiliate.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ việc phát triển cộng đồng";
                            newNoti1.system_type = "WALLET";
                            newNoti1.reference_id = null;

                            _context.Notifications.Add(newNoti1);
                            _context.SaveChanges();
                        }
                    }
                }

                // Cộng điểm hệ thống
                var newSystemPointHistory = new SystemPointHistory();
                newSystemPointHistory.id = Guid.NewGuid();
                newSystemPointHistory.order_type = "PUSH";
                newSystemPointHistory.point_amount = pointSystem;
                newSystemPointHistory.status = 4;
                newSystemPointHistory.trans_date = DateTime.Now;

                if (isDebit == true)
                {
                    newSystemPointHistory.point_type = "WAITING";
                }
                else
                {
                    newSystemPointHistory.point_type = "AVAIABLE";
                }

                _context.SystemPointHistorys.Add(newSystemPointHistory);

                _context.SaveChanges();


                // Lưu thông tin điểm hóa đơn
                request.status = 5;
                request.discount_rate = contractObj.discount_rate;
                request.point_exchange = total_point;
                if (isDebit == true)
                {
                    request.point_waiting = total_point;
                    request.point_avaiable = 0;
                }
                else
                {
                    request.point_avaiable = total_point;
                    request.point_waiting = 0;
                }
                request.point_partner = total_point;
                request.point_customer = pointCustomer;
                request.point_system = pointSystem;
                request.approve_user = username;
                request.user_updated = username;
                request.approve_date = DateTime.Now;
                request.date_updated = DateTime.Now;
                _context.SaveChanges();

                // Tạo detail
                for (int i = 0; i < listAccuDetails.Count; i++)
                {
                    var newDetail = new AccumulatePointOrderDetail();
                    newDetail.id = Guid.NewGuid();
                    newDetail.accumulate_point_order_id = request.id;
                    newDetail.name = listAccuDetails[i].name;
                    newDetail.discount_rate = listAccuDetails[i].discount_rate;
                    if (listAccuDetails[i].name == "Khách hàng")
                    {
                        newDetail.point_value = pointCustomer;
                        newDetail.allocation_name = "Người tiêu dùng";
                        newDetail.description = "Chiết khấu cho NTD";
                    }
                    else if (listAccuDetails[i].name == "Hệ thống")
                    {
                        newDetail.point_value = pointSystem;
                        newDetail.allocation_name = "Hệ thống(Admin)";
                        newDetail.description = "Chiết khấu cho CashPlus";
                    }
                    else if (listAccuDetails[i].name == "Điểm tích lũy")
                    {
                        newDetail.point_value = pointAffiliate;
                        newDetail.allocation_name = "Điểm tích lũy";
                        newDetail.description = "Chiết khấu cho người giưới thiệu sẽ được chuyển về CashPlus, từ đó CashPlus sẽ cộng điểm tích lũy cho NGT";
                    }

                    _context.AccumulatePointOrderDetails.Add(newDetail);
                }

                _context.SaveChanges();

                // Gửi FCM
                // Cho khách
                if (userCustomer.device_id != null && userCustomer.send_Notification == true)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userCustomer.customer_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    if (request.return_type == "Cash")
                    {
                        newNoti1.title = "Hoàn tiền tiêu dùng";
                        //newNoti1.content = "Tài khoản của bạn vừa nhận được " + (pointCustomer * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " VNĐ từ giao dịch " + request.trans_no + " với số tiền thanh toán là: " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " vào lúc " + _commonFunction.convertDateToStringFull(request.date_created);

                        var notiReturn = notiConfig.Payment_Refund;
                        newNoti1.content = notiReturn.Replace("{TienHoan}", (pointCustomer * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{MaGiaoDich}", request.trans_no).Replace("{SoTienGiaoDich}", ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(request.date_created));
                    }
                    else
                    {
                        newNoti1.title = "Tích điểm tiêu dùng";
                        //newNoti1.content = "Tài khoản của bạn vừa nhận được " + pointCustomer.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " điểm thưởng từ giao dịch " + request.trans_no + " với số tiền thanh toán là: " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " vào lúc " + _commonFunction.convertDateToStringFull(request.date_created);
                        var notiReturn = notiConfig.Payment_NotEnRefund;
                        newNoti1.content = notiReturn.Replace("{DiemTD}", pointCustomer.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{MaGiaoDich}", request.trans_no).Replace("{SoTienGiaoDich}", ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(request.date_created));
                    }
                    newNoti1.system_type = "ACCU_POINT";
                    newNoti1.reference_id = request.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();
                    if (userCustomer.send_Notification == true)
                    {
                        _fCMNotification.SendNotification(userCustomer.device_id,
                                                   "ACCU_POINT",
                                                   newNoti1.title,
                                                  newNoti1.content,
                                                   request.id);
                    }
                }
                // Cho shop
                if (userPartner.device_id != null && userPartner.send_Notification == true)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Hoàn tiền tiêu dùng";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userPartner.partner_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    //newNoti1.content = "Tài khoản của bạn đã bị trừ " + (total_point * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + "VNĐ từ giao dịch " + request.trans_no + " xác nhậm thanh toán số tiền " + ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." });
                    var returnNoti = notiConfig.MC_Payment_Refund;
                    newNoti1.content = returnNoti.Replace("{TongTienChietKhau}", (total_point * pointExchangeRate).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{MaGiaoDich}", request.trans_no).Replace("{SoTienGiaoDich}", ((decimal)request.bill_amount).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }));
                    newNoti1.system_type = "ACCU_POINT";
                    newNoti1.reference_id = request.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();
                    if (userPartner.send_Notification == true)
                    {
                        _fCMNotification.SendNotification(userPartner.device_id,
                                                    "ACCU_POINT",
                                                    newNoti1.title,
                                                    newNoti1.content,
                                                    request.id);
                    }
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(ex.Message);
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(new
            {
                total_scan_qr = total_scan_qr != null ? (total_scan_qr + 1) : 1
            });
        }
    }
}
