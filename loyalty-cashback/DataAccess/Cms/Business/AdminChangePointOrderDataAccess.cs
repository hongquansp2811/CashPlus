using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.PaymentGate;
using LOYALTY.CloudMessaging;
using System.Globalization;

namespace LOYALTY.DataAccess
{
    public class AdminChangePointDataAccess : IAdminChangePointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly ICommon _common;
        private readonly BKTransaction _bkTransaction;
        private readonly SysTransDataAccess _sysTransDataAccess;
        private readonly IEmailSender _emailSender;
        private readonly FCMNotification _fCMNotification;
        public AdminChangePointDataAccess(LOYALTYContext context, ICommonFunction commonFunction, ICommon common, BKTransaction bkTransaction, SysTransDataAccess sysTranDataAccess, IEmailSender emailSender, FCMNotification fCMNotification)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _common = common;
            _bkTransaction = bkTransaction;
            _sysTransDataAccess = sysTranDataAccess;
            _emailSender = emailSender;
            _fCMNotification = fCMNotification;
        }

        public APIResponse getList(ChangePointOrderRequest request)
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

            var lstData = (from p in _context.ChangePointOrders
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           join c in _context.Customers on p.user_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on p.user_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               user_type = c != null ? "CUSTOMER" : "PARTNER",
                               user_type_name = c != null ? "Khách hàng" : "Đối tác",
                               username = c != null ? c.phone : s.username,
                               trans_no = p.trans_no,
                               trans_date = p.date_created,
                               trans_date_2 = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               point_exchange = p.point_exchange,
                               value_exchange = p.value_exchange,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()) || x.username.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.user_type != null)
            {
                lstData = lstData.Where(x => x.user_type == request.user_type);
            }

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
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            if (request.from_point != null)
            {
                lstData = lstData.Where(x => x.point_exchange >= request.from_point);
            }

            if (request.to_point != null)
            {
                lstData = lstData.Where(x => x.point_exchange <= request.to_point);
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
            var data = (from p in _context.ChangePointOrders
                        join cb in _context.CustomerBankAccounts on p.customer_bank_account_id equals cb.id into cbs
                        from cb in cbs.DefaultIfEmpty()
                        join b in _context.Banks on cb.bank_id equals b.id into bs
                        from b in bs.DefaultIfEmpty()
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        join c in _context.Customers on p.user_id equals c.id into cs
                        from c in cs.DefaultIfEmpty()
                        join s in _context.Partners on p.user_id equals s.id into ss
                        from s in ss.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            user_type = c != null ? "CUSTOMER" : "PARTNER",
                            user_type_name = c != null ? "Khách hàng" : "Đối tác",
                            username = c != null ? c.phone : s.username,
                            partner_code = s != null ? s.code : "",
                            partner_name = s != null ? s.name : "",
                            partner_phone = s != null ? s.phone : "",
                            customer_name = c != null ? c.full_name : "",
                            customer_phone = c != null ? c.phone : "",
                            trans_no = p.trans_no,
                            trans_date = p.date_created,
                            bank_name = b.name,
                            bank_no = cb.bank_no,
                            bank_owner = cb.bank_owner,
                            point_exchange = p.point_exchange,
                            value_exchange = p.value_exchange,
                            status = p.status,
                            status_name = st != null ? st.name : ""
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse approve(ChangePointOrderRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }

            var data = _context.ChangePointOrders.Where(x => x.id == request.id).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_ID_INCORRECT");
            }

            if (data.status != 4)
            {
                return new APIResponse("ERROR_ORDER_NOT_UPDATE");
            }

            if (request.approve_date == null)
            {
                return new APIResponse("ERROR_APPROVE_DATE_MISSING");
            }

            if (request.status == 6)
            {
                if (request.reason_fail == null)
                {
                    return new APIResponse("ERROR_REASON_FAIL_MISSING");
                }
            }

            var userCustomerObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == data.user_id).FirstOrDefault();
            var userPartnerObj = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == data.user_id).FirstOrDefault();

            if (userCustomerObj == null && userPartnerObj == null)
            {
                return new APIResponse("ERROR_ORDER_USER_INCORRECT");
            }

            var userType = "CUSTOMER";
            if (userPartnerObj != null)
            {
                userType = "PARTNER";

                if (userPartnerObj.point_avaiable < data.point_exchange)
                {
                    return new APIResponse("ERROR_POINT_NOT_ENOUGH");
                }
            }
            else
            {
                if (userCustomerObj.point_avaiable < data.point_exchange)
                {
                    return new APIResponse("ERROR_POINT_NOT_ENOUGH");
                }
            }

            var objBankAccount = (from p in _context.CustomerBankAccounts
                                  join b in _context.Banks on p.bank_id equals b.id
                                  where p.user_id == data.user_id && p.id == data.customer_bank_account_id
                                  select new
                                  {
                                      bank_no = p.bank_no,
                                      bank_owner = p.bank_owner,
                                      bank_code = b.bank_code,
                                      bank_name = b.name
                                  }).FirstOrDefault();

            if (objBankAccount == null)
            {
                return new APIResponse("ERROR_BANK_ACCOUNT_INCORRECT");
            }
            var transaction = _context.Database.BeginTransaction();

            try
            {
                // Cập nhật hóa đơn
                data.status = request.status;
                data.approve_date = _commonFunction.convertStringSortToDate(request.approve_date);
                data.reason_fail = request.reason_fail;
                _context.SaveChanges();
                var notiConfig = _context.NotiConfigs.FirstOrDefault();
                if (request.status == 5)
                {
                    // Trừ điểm tài khoản
                    if (userType == "CUSTOMER")
                    {
                        userCustomerObj.point_avaiable -= data.point_exchange;
                        userCustomerObj.total_point = userCustomerObj.point_avaiable + userCustomerObj.point_waiting + userCustomerObj.point_affiliate;

                        _context.SaveChanges();

                        // Tạo bảng CustomerPointHistory
                        var newCustomerPointHistory = new CustomerPointHistory();
                        newCustomerPointHistory.id = Guid.NewGuid();
                        newCustomerPointHistory.order_type = "CHANGE_POINT";
                        newCustomerPointHistory.customer_id = data.user_id;
                        newCustomerPointHistory.point_amount = data.point_exchange;
                        newCustomerPointHistory.status = 5;
                        newCustomerPointHistory.trans_date = DateTime.Now;
                        newCustomerPointHistory.point_type = "AVAIABLE";
                        _context.CustomerPointHistorys.Add(newCustomerPointHistory);

                        _context.SaveChanges();

                        _common.checkUpdateCustomerRank((Guid)userCustomerObj.id);
                    }
                    else
                    {
                        userPartnerObj.point_avaiable -= data.point_exchange;
                        userPartnerObj.total_point = userPartnerObj.point_avaiable + userPartnerObj.point_waiting + userPartnerObj.point_affiliate;

                        _context.SaveChanges();

                        // Tạo bảng CustomerPointHistory
                        var newHistory = new PartnerPointHistory();
                        newHistory.id = Guid.NewGuid();
                        newHistory.order_type = "CHANGE_POINT";
                        newHistory.partner_id = data.user_id;
                        newHistory.point_amount = data.point_exchange;
                        newHistory.status = 5;
                        newHistory.trans_date = DateTime.Now;
                        newHistory.point_type = "AVAIABLE";
                        _context.PartnerPointHistorys.Add(newHistory);

                        _context.SaveChanges();
                    }

                    // Gọi API trừ tiền
                    _bkTransaction.transferMoney(Consts.CP_BK_PARTNER_CODE, objBankAccount.bank_code, objBankAccount.bank_no, objBankAccount.bank_owner, (decimal)data.value_exchange, "Chuyen tien", Consts.private_key);

                    // Vào dòng tiền
                    _sysTransDataAccess.insertSystemTrans(DateTime.Now, "PL_CUS_CHANGE_POINT", "PULL", (decimal)data.value_exchange, true);

                    // Gửi mail cho tài khoản
                    string mail_to = "";
                    if (userType == "CUSTOMER")
                    {
                        var customerObj = _context.Customers.Where(x => x.id == userCustomerObj.customer_id).FirstOrDefault();
                        if (customerObj != null && customerObj.email != null)
                        {
                            mail_to = customerObj.email;
                        }
                    }
                    else
                    {
                        var partnerObj = _context.Partners.Where(x => x.id == userPartnerObj.partner_id).FirstOrDefault();
                        if (partnerObj != null && partnerObj.email != null)
                        {
                            mail_to = partnerObj.email;
                        }
                    }
                    if (mail_to.Length > 0)
                    {
                        string subjectEmail = "[CashPlus] - Thông báo duyệt phiếu đổi điểm";
                        string message = "<p>Xin chào " + (userType == "CUSTOMER" ? userCustomerObj.full_name : userPartnerObj.full_name) + "!<p>";
                        message += "<br/>";
                        //message += "<p>Phiếu đổi điểm Mã giao dịch " + data.trans_no + " với số điểm quy đổi là:  " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " của bạn đã được duyệt vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now) + "</p>";
                        message += "<p>" + notiConfig.MC_ChangePoint_Acp.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)) + "</p>";
                        message += "<br/>";
                        message += "<p>Thông tin người thụ hưởng:</p>";
                        message += "<p>- Số tiền: " + ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + "</p>";
                        message += "<p>- Ngân hàng: " + objBankAccount.bank_name + "</p>";
                        message += "<p>- Số tài khoản: " + objBankAccount.bank_no + "</p>";
                        message += "<p>- Chủ tài khoản: " + objBankAccount.bank_owner + "</p>";
                        message += "<br/>";
                        message += "<p>Trân trọng!</p>";
                        message += "<br/>";
                        message += "<p>@2023 ATS Group</p>";

                        _emailSender.SendEmailAsync(mail_to, subjectEmail, message);
                    }

                    // Gửi firebase cho khách hàng đối tác
                    if (userType == "CUSTOMER" && userCustomerObj.device_id != null)
                    {
                        var newNoti1 = new Notification();
                        newNoti1.id = Guid.NewGuid();
                        newNoti1.title = "Duyệt phiếu đổi điểm thành công";
                        newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                        newNoti1.user_id = userCustomerObj.customer_id;
                        newNoti1.date_created = DateTime.Now;
                        newNoti1.date_updated = DateTime.Now;
                        //newNoti1.content = "Phiếu đổi điểm Mã giao dịch " + data.trans_no + " với số điểm quy đổi là: " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " của bạn đã được duyệt vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now);
                        newNoti1.content =  notiConfig.MC_ChangePoint_Acp.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now));

                        newNoti1.system_type = "CHANGE_POINT";
                        newNoti1.reference_id = data.id;

                        _context.Notifications.Add(newNoti1);
                        _context.SaveChanges();

                        _fCMNotification.SendNotification(userCustomerObj.device_id,
                           "CHANGE_POINT",
                           "Duyệt phiếu đổi điểm thành công",
                           newNoti1.content,
                           data.id);
                    }
                    else if (userType == "PARTNER" && userPartnerObj.device_id != null)
                    {
                        var newNoti1 = new Notification();
                        newNoti1.id = Guid.NewGuid();
                        newNoti1.title = "Duyệt phiếu đổi điểm thành công";
                        newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                        newNoti1.user_id = userPartnerObj.partner_id;
                        newNoti1.date_created = DateTime.Now;
                        newNoti1.date_updated = DateTime.Now;
                        newNoti1.content = notiConfig.MC_ChangePoint_Acp.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now));
                        newNoti1.system_type = "CHANGE_POINT";
                        newNoti1.reference_id = data.id;

                        _context.Notifications.Add(newNoti1);
                        _context.SaveChanges();


                        _fCMNotification.SendNotification(userPartnerObj.device_id,
                            "CHANGE_POINT",
                            "Duyệt phiếu đổi điểm thành công",
                            notiConfig.MC_ChangePoint_Acp.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)),
                            data.id);
                    }

                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_ADD_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(data);
        }

    }
}
