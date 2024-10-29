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
    public class AppChangePointDataAccess : IAppChangePointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly ICommon _common;
        private readonly BKTransaction _bkTransaction;
        private readonly SysTransDataAccess _sysTransDataAccess;
        private readonly IEmailSender _emailSender;
        private readonly FCMNotification _fCMNotification;
        public AppChangePointDataAccess(LOYALTYContext context, ICommonFunction commonFunction, ICommon common, BKTransaction bKTransaction, SysTransDataAccess sysTransDataAccess, IEmailSender emailSender, FCMNotification fCMNotification)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _common = common;
            _bkTransaction = bKTransaction;
            _sysTransDataAccess = sysTransDataAccess;
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
                           where p.user_id == request.user_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = p.date_created,
                               trans_date_origin = p.date_created,
                               point_exchange = p.point_exchange,
                               value_exchange = p.value_exchange,
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
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
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
                        join cb in _context.CustomerBankAccounts on p.customer_bank_account_id equals cb.id
                        join b in _context.Banks on cb.bank_id equals b.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            trans_no = p.trans_no,
                            trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                            bank_name = b.name,
                            bank_no = cb.bank_no,
                            bank_owner = cb.bank_owner,
                            point_exchange = p.point_exchange,
                            value_exchange = p.value_exchange,
                            status = p.status,
                            status_name = st != null ? st.name : "",
                            reason_fail = p.reason_fail
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(ChangePointOrderRequest request, string username)
        {
            if (request.user_id == null)
            {
                return new APIResponse("ERROR_USER_ID_MISSING");
            }

            if (request.point_exchange == null)
            {
                return new APIResponse("ERROR_POINT_EXCHANGE_MISSING");
            }

            var userCustomerObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == request.user_id).FirstOrDefault();
            var userPartnerObj = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == request.user_id).FirstOrDefault();
            var userPartnerSendObj = _context.Users.Where(x => x.is_partner == true && x.username == username && x.partner_id == request.user_id).FirstOrDefault();

            if (userCustomerObj == null && userPartnerObj == null)
            {
                return new APIResponse("ERROR_ORDER_USER_INCORRECT");
            }

            var userType = "CUSTOMER";
            if (userPartnerObj != null)
            {
                userType = "PARTNER";

                if (userPartnerObj.point_avaiable < request.point_exchange)
                {
                    return new APIResponse("ERROR_POINT_NOT_ENOUGH");
                }
            }
            else
            {
                if (userCustomerObj.point_avaiable < request.point_exchange)
                {
                    return new APIResponse("ERROR_POINT_NOT_ENOUGH");
                }
            }

            var objBankAccount = (from p in _context.CustomerBankAccounts
                                  join b in _context.Banks on p.bank_id equals b.id
                                  where p.user_id == request.user_id && p.id == request.customer_bank_account_id
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

            var dateNow = DateTime.Now;

            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null || settingObj.change_point_estimate == null)
            {
                return new APIResponse("ERROR_SETTINGS_NOT_CONFIG");
            }
            decimal pointExchangeRate = Math.Round(((decimal)settingObj.point_exchange / (decimal)settingObj.point_value), 2);

            var total_change_point = 4;
            if (userType == "CUSTOMER")
            {
                total_change_point = _context.ChangePointOrders.Where(x => x.user_id == request.user_id && x.status == 5).Count();
            }

            if (total_change_point > 3 && request.point_exchange < settingObj.change_point_estimate)
            {
                return new APIResponse("ERROR_CHANGE_POINT_NOT_ENOUGH");
            }

            decimal value_exchange = (decimal)request.point_exchange * pointExchangeRate;
            var transaction = _context.Database.BeginTransaction();

            Guid orderId = Guid.NewGuid();
            var data = new ChangePointOrder();
            var maxCodeObject = _context.ChangePointOrders.Where(x => x.trans_no != null && x.trans_no.Contains("DD")).OrderByDescending(x => x.trans_no).FirstOrDefault();
            string codeOrder = "";
            if (maxCodeObject == null)
            {
                codeOrder = "DD0000000000001";
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
                codeOrder = "DD" + orderString.PadLeft(number, pad);
            }

            try
            {
                bool is_approve = false;
                // Nếu nhỏ hơn điểm tự động duyệt thì thực hiện tự động duyệt
                if (settingObj != null && settingObj.approve_change_point_min != null && request.point_exchange <= settingObj.approve_change_point_min)
                {
                    // Trừ điểm tài khoản
                    if (userType == "CUSTOMER")
                    {
                        userCustomerObj.point_avaiable -= request.point_exchange;
                        userCustomerObj.total_point = userCustomerObj.point_avaiable + userCustomerObj.point_waiting + userCustomerObj.point_affiliate;

                        _context.SaveChanges();

                        // Tạo bảng CustomerPointHistory
                        var newCustomerPointHistory = new CustomerPointHistory();
                        newCustomerPointHistory.id = Guid.NewGuid();
                        newCustomerPointHistory.order_type = "CHANGE_POINT";
                        newCustomerPointHistory.customer_id = request.user_id;
                        newCustomerPointHistory.point_amount = request.point_exchange;
                        newCustomerPointHistory.status = 5;
                        newCustomerPointHistory.trans_date = DateTime.Now;
                        newCustomerPointHistory.point_type = "AVAIABLE";
                        _context.CustomerPointHistorys.Add(newCustomerPointHistory);

                        _context.SaveChanges();

                        _common.checkUpdateCustomerRank((Guid)userCustomerObj.id);
                    }
                    else
                    {
                        userPartnerObj.point_avaiable -= request.point_exchange;
                        userPartnerObj.total_point = userPartnerObj.point_avaiable + userPartnerObj.point_waiting + userPartnerObj.point_affiliate;

                        _context.SaveChanges();

                        // Tạo bảng CustomerPointHistory
                        var newHistory = new PartnerPointHistory();
                        newHistory.id = Guid.NewGuid();
                        newHistory.order_type = "CHANGE_POINT";
                        newHistory.partner_id = request.user_id;
                        newHistory.point_amount = request.point_exchange;
                        newHistory.status = 5;
                        newHistory.trans_date = DateTime.Now;
                        newHistory.point_type = "AVAIABLE";
                        _context.PartnerPointHistorys.Add(newHistory);

                        _context.SaveChanges();
                    }

                    // Gọi API trừ tiền
                    _bkTransaction.transferMoney(Consts.CP_BK_PARTNER_CODE, objBankAccount.bank_code, objBankAccount.bank_no, objBankAccount.bank_owner, value_exchange, "Chuyen tien", Consts.private_key);

                    // Vào dòng tiền
                    _sysTransDataAccess.insertSystemTrans(DateTime.Now, "PL_CUS_CHANGE_POINT", "PULL", value_exchange, true);

                    is_approve = true;
                }

                data.id = orderId;
                data.trans_no = codeOrder;
                data.user_id = request.user_id;
                data.trans_type_id = request.trans_type_id;
                data.value_exchange = value_exchange;
                data.point_exchange = request.point_exchange;
                data.exchange_rate = pointExchangeRate;
                data.customer_bank_account_id = request.customer_bank_account_id;
                data.status = is_approve == true ? 5 : 4;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.ChangePointOrders.Add(data);
                _context.SaveChanges();

                var notiConfig = _context.NotiConfigs.FirstOrDefault();

                // Tạo thông báo cho admin
                var newNotiAdmin = new Notification();
                newNotiAdmin.id = Guid.NewGuid();
                newNotiAdmin.title = "Yêu cầu duyệt phiếu đổi điểm";
                newNotiAdmin.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                newNotiAdmin.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNotiAdmin.date_created = DateTime.Now;
                newNotiAdmin.date_updated = DateTime.Now;
                //newNotiAdmin.content = "Tài khoản " + username + "vừa thực hiện thêm mới phiếu đổi điểm với số điểm quy đổi là: " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + ". Mã giao dịch " + data.trans_no + " vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now);
                newNotiAdmin.content = notiConfig.ChangePoint_Add.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now));
                newNotiAdmin.system_type = "CHANGE_POINT";
                newNotiAdmin.reference_id = data.id;

                _context.Notifications.Add(newNotiAdmin);
                _context.SaveChanges();

                // Gửi mail cho admin
                string subjectEmail = "[CashPlus] - Thông báo phiếu đổi điểm";
                string mail_to = Consts.ADMIN_MAIL;
                string message = "<p>Xin chào!<p>";
                //message += "<p>Tài khoản " + username + " vừa thực hiện thêm mới phiếu đổi điểm với số điểm quy đổi là: " + request.point_exchange + " tương đương " + value_exchange + " .  Mã giao dịch " + codeOrder + " vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now) + "</p>";
                message +="<p>" + notiConfig.ChangePoint_Add.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)) + "</p>";
                message += "<p>Trân trọng!</p>";
                message += "<br/>";
                message += "<p>@2023 ATS Group</p>";

                _emailSender.SendEmailAsync(mail_to, subjectEmail, message);

                // Gửi mail cho tài khoản
                string mail_to_2 = "";
                if (userType == "CUSTOMER")
                {
                    var customerObj = _context.Customers.Where(x => x.id == userCustomerObj.customer_id).FirstOrDefault();
                    if (customerObj != null && customerObj.email != null)
                    {
                        mail_to_2 = customerObj.email;
                    }
                }
                else
                {
                    var partnerObj = _context.Partners.Where(x => x.id == userPartnerObj.partner_id).FirstOrDefault();
                    if (partnerObj != null && partnerObj.email != null)
                    {
                        mail_to_2 = partnerObj.email;
                    }
                }
                if (mail_to_2.Length > 0 && request.point_exchange <= settingObj.approve_change_point_min)
                {
                    string subjectEmail2 = "[CashPlus] - Thông báo duyệt phiếu đổi điểm";
                    string message2 = "<p>Xin chào " + (userType == "CUSTOMER" ? userCustomerObj.full_name : userPartnerObj.full_name) + "!<p>";
                    message2 += "<br/>";
                    //message2 += "<p>Phiếu đổi điểm Mã giao dịch " + data.trans_no + " với số điểm quy đổi là:  " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + data.value_exchange + " của bạn đã được duyệt vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now) + "</p>";
                    message2 += "<p>" + notiConfig.MC_ChangePoint_Acp.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)) + "</p>";

                    message2 += "<br/>";
                    message2 += "<p>Thông tin người thụ hưởng:</p>";
                    message2 += "<p>- Số tiền: " + data.value_exchange + "</p>";
                    message2 += "<p>- Ngân hàng: " + objBankAccount.bank_name + "</p>";
                    message2 += "<p>- Số tài khoản: " + objBankAccount.bank_no + "</p>";
                    message2 += "<p>- Chủ tài khoản: " + objBankAccount.bank_owner + "</p>";
                    message2 += "<br/>";
                    message2 += "<p>Trân trọng!</p>";
                    message2 += "<br/>";
                    message2 += "<p>@2023 ATS Group</p>";

                    _emailSender.SendEmailAsync(mail_to_2, subjectEmail2, message2);
                }

                // Gửi firebase cho khách hàng đối tác
                if (userType == "CUSTOMER" && userCustomerObj.device_id != null)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Tạo phiếu đổi điểm thành công";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userCustomerObj.customer_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    //newNoti1.content = "Phiếu đổi điểm Mã giao dịch " + data.trans_no + " với số điểm quy đổi là: " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " của bạn đã được tạo vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now);
                    newNoti1.content = notiConfig.ChangePoint_Add.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now));
                    newNoti1.system_type = "CHANGE_POINT";
                    newNoti1.reference_id = data.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    _fCMNotification.SendNotification(userCustomerObj.device_id,
                       "CHANGE_POINT",
                       "Tạo phiếu đổi điểm thành công",
                       newNoti1.content,
                       data.id);
                }
                else if (userType == "PARTNER")
                {
                    if (userPartnerObj.device_id != null)
                    {
                        var newNoti1 = new Notification();
                        newNoti1.id = Guid.NewGuid();
                        newNoti1.title = "Tạo phiếu đổi điểm thành công";
                        newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                        newNoti1.user_id = userPartnerObj.partner_id;
                        newNoti1.date_created = DateTime.Now;
                        newNoti1.date_updated = DateTime.Now;
                        //newNoti1.content = "Phiếu đổi điểm Mã giao dịch " + data.trans_no + " với số điểm quy đổi là: " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " của bạn đã được tạo vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now);
                        newNoti1.content = notiConfig.MC_ChangePoint_Add.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now));
                        newNoti1.system_type = "CHANGE_POINT";
                        newNoti1.reference_id = data.id;

                        _context.Notifications.Add(newNoti1);
                        _context.SaveChanges();

                        _fCMNotification.SendNotification(userPartnerObj.device_id,
                            "CHANGE_POINT",
                            "Tạo phiếu đổi điểm thành công",
                            newNoti1.content,
                            data.id);
                    }

                    if (userPartnerSendObj != null && userPartnerSendObj.device_id != null)
                    {

                        _fCMNotification.SendNotification(userPartnerSendObj.device_id,
                            "CHANGE_POINT",
                            "Tạo phiếu đổi điểm thành công",
                            notiConfig.MC_ChangePoint_Add.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)),
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

            var dataReturn = (from p in _context.ChangePointOrders
                              join cb in _context.CustomerBankAccounts on p.customer_bank_account_id equals cb.id
                              join b in _context.Banks on cb.bank_id equals b.id
                              join st in _context.OtherLists on p.status equals st.id into sts
                              from st in sts.DefaultIfEmpty()
                              where p.id == data.id
                              select new
                              {
                                  id = p.id,
                                  trans_no = p.trans_no,
                                  trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                                  bank_name = b.name,
                                  bank_no = cb.bank_no,
                                  bank_owner = cb.bank_owner,
                                  point_exchange = p.point_exchange,
                                  value_exchange = p.value_exchange,
                                  status = p.status,
                                  status_name = st != null ? st.name : ""
                              }).FirstOrDefault();
            return new APIResponse(dataReturn);
        }

        public APIResponse cancelChangePoint(ChangePointOrderRequest request, string username)
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

            var userCustomerObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == data.user_id).FirstOrDefault();
            var userPartnerObj = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == data.user_id).FirstOrDefault();
            var userPartnerSendObj = _context.Users.Where(x => x.is_partner == true && x.username == username && x.partner_id == data.user_id).FirstOrDefault();

            if (userCustomerObj == null && userPartnerObj == null)
            {
                return new APIResponse("ERROR_ORDER_USER_INCORRECT");
            }

            var userType = "CUSTOMER";
            if (userPartnerObj != null)
            {
                userType = "PARTNER";
            }

            try
            {
                data.status = 18;
                data.date_updated = DateTime.Now;
                data.user_updated = username;
                _context.SaveChanges();

                var notiConfig = _context.NotiConfigs.FirstOrDefault();

                // Gửi firebase cho khách hàng đối tác
                if (userType == "CUSTOMER" && userCustomerObj.device_id != null)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Hủy phiếu đổi điểm thành công";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userCustomerObj.customer_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    newNoti1.content = notiConfig.ChangePoint_De.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)).Replace("{LyDoHuyDuyet}", data.reason_fail);
                    //newNoti1.content = "Phiếu đổi điểm Mã giao dịch " + data.trans_no + " với số điểm quy đổi là: " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " của bạn đã được hủy vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now);
                    newNoti1.system_type = "CHANGE_POINT";
                    newNoti1.reference_id = data.id;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    _fCMNotification.SendNotification(userCustomerObj.device_id,
                       "CHANGE_POINT",
                       "Hủy phiếu đổi điểm thành công",
                       notiConfig.ChangePoint_De.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)).Replace("{LyDoHuyDuyet}", data.reason_fail),
                       data.id);
                }
                else if (userType == "PARTNER")
                {
                    if (userPartnerObj.device_id != null)
                    {
                        var newNoti1 = new Notification();
                        newNoti1.id = Guid.NewGuid();
                        newNoti1.title = "Hủy đổi điểm thành công";
                        newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                        newNoti1.user_id = userPartnerObj.partner_id;
                        newNoti1.date_created = DateTime.Now;
                        newNoti1.date_updated = DateTime.Now;
                        newNoti1.content = notiConfig.MC_ChangePoint_De.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)).Replace("{LyDoHuyDuyet}", data.reason_fail);
                        newNoti1.system_type = "CHANGE_POINT";
                        newNoti1.reference_id = data.id;

                        _context.Notifications.Add(newNoti1);
                        _context.SaveChanges();

                        _fCMNotification.SendNotification(userPartnerObj.device_id,
                            "CHANGE_POINT",
                            "Hủy phiếu đổi điểm thành công",
                            notiConfig.MC_ChangePoint_De.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)).Replace("{LyDoHuyDuyet}", data.reason_fail),
                            data.id);
                    }

                    if (userPartnerSendObj != null && userPartnerSendObj.device_id != null)
                    {

                        _fCMNotification.SendNotification(userPartnerSendObj.device_id,
                            "CHANGE_POINT",
                            "Hủy phiếu đổi điểm thành công",
                            notiConfig.MC_ChangePoint_De.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)).Replace("{LyDoHuyDuyet}", data.reason_fail),
                            data.id);
                    }


                    string subjectEmail2 = "[CashPlus] - Thông báo duyệt phiếu đổi điểm";
                    string message2 = "<p>Xin chào " + (userType == "CUSTOMER" ? userCustomerObj.full_name : userPartnerObj.full_name) + "!<p>";
                    message2 += "<br/>";
                    //message2 += "<p>Phiếu đổi điểm Mã giao dịch " + data.trans_no + " với số điểm quy đổi là:  " + ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }) + " tương đương " + data.value_exchange + " của bạn đã được duyệt vào lúc " + _commonFunction.convertDateToStringFull(DateTime.Now) + "</p>";
                    message2 += "<p>" + notiConfig.ChangePoint_De.Replace("{Ma_GD_DD}", data.trans_no).Replace("{DD_DTD}", ((decimal)data.point_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{DD_TongTien}", ((decimal)data.value_exchange).ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)).Replace("{LyDoHuyDuyet}", data.reason_fail) + "</p>";
                    message2 += "<p>Trân trọng!</p>";
                    message2 += "<br/>";
                    message2 += "<p>@2023 ATS Group</p>";

                    _emailSender.SendEmailAsync(userType == "CUSTOMER" ? userCustomerObj.email : userPartnerObj.email, subjectEmail2, message2);
                }
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }
            return new APIResponse(200);
        }

        public APIResponse getExchangePack()
        {
            var data = _context.ExchangePointPackConfigs.ToList();

            return new APIResponse(data);
        }
    }
}
