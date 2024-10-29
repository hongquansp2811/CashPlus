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

namespace LOYALTY.DataAccess
{
    public class RecallPointOrderDataAccess : IRecallPointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly BKTransaction _bkTransaction;
        private readonly SysTransDataAccess _sysTransDataAccess;
        private readonly IEmailSender _emailSender;
        public RecallPointOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction, BKTransaction bkTransaction, SysTransDataAccess sysTranDataAccess, IEmailSender emailSender)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _bkTransaction = bkTransaction;
            _sysTransDataAccess = sysTranDataAccess;
            _emailSender = emailSender;
        }

        public APIResponse getListBlackUser(UserRequest request)
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

            var lstData = (from p in _context.Users
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           where p.is_violation == true
                           select new
                           {
                               id = p.id,
                               user_type = c != null ? "CUSTOMER" : "PARTNER",
                               user_type_name = c != null ? "Khách hàng" : "Đối tác",
                               customer_id = c != null ? c.id : null,
                               partner_id = s != null ? s.id : null,
                               username = p.username,
                               total_point = p.total_point,
                               point_waiting = p.point_waiting,
                               point_avaiable = p.point_avaiable,
                               point_affiliate = p.point_affiliate
                           });

            if (request.username != null)
            {
                lstData = lstData.Where(x => x.username.Trim().ToLower().Contains(request.username.Trim().ToLower()));
            }

            if (request.user_type != null)
            {
                lstData = lstData.Where(x => x.user_type == request.user_type);
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

        public APIResponse create(RecallPointOrderRequest request, string username)
        {
            if (request.user_id == null)
            {
                return new APIResponse("ERROR_APPROVE_DATE_MISSING");
            }

            var userCustomerObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == request.user_id).FirstOrDefault();
            var userPartnerObj = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == request.user_id).FirstOrDefault();

            if (userCustomerObj == null && userPartnerObj == null)
            {
                return new APIResponse("ERROR_ORDER_USER_INCORRECT");
            }
            decimal point_subtract = 0;
            var userType = "CUSTOMER";
            if (userPartnerObj != null)
            {
                userType = "PARTNER";
                if (userPartnerObj.point_avaiable < 0)
                {
                    point_subtract = (decimal)userPartnerObj.point_affiliate + (decimal)userPartnerObj.point_waiting;
                }
                else
                {
                    point_subtract = (decimal)userPartnerObj.total_point;
                }
            }
            else
            {
                if (userCustomerObj.point_avaiable < 0)
                {
                    point_subtract = (decimal)userCustomerObj.point_affiliate + (decimal)userCustomerObj.point_waiting;
                }
                else
                {
                    point_subtract = (decimal)userCustomerObj.total_point;
                }
            }

            var transaction = _context.Database.BeginTransaction();

            try
            {
                // Tạo chứng từ
                Guid orderId = Guid.NewGuid();
                var data = new RecallPointOrder();

                var maxCodeObject = _context.RecallPointOrders.Where(x => x.trans_no != null && x.trans_no.Contains("TH")).OrderByDescending(x => x.trans_no).FirstOrDefault();
                string codeOrder = "";
                if (maxCodeObject == null)
                {
                    codeOrder = "TH0000000000001";
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
                    codeOrder = "TH" + orderString.PadLeft(number, pad);
                }

                data.id = orderId;
                data.user_id = request.user_id;
                data.point_value = point_subtract;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.RecallPointOrders.Add(data);
                _context.SaveChanges();


                // Trừ điểm tài khoản
                if (userType == "CUSTOMER")
                {
                    if (userCustomerObj.point_avaiable > 0)
                    {
                        userCustomerObj.point_avaiable = 0;
                    }
                    userCustomerObj.point_waiting = 0;
                    userCustomerObj.point_affiliate = 0;
                    userCustomerObj.total_point = userCustomerObj.point_avaiable;
                    _context.SaveChanges();

                    // Tạo bảng CustomerPointHistory
                    var newCustomerPointHistory = new CustomerPointHistory();
                    newCustomerPointHistory.id = Guid.NewGuid();
                    newCustomerPointHistory.order_type = "RECALL_POINT";
                    newCustomerPointHistory.customer_id = userCustomerObj.customer_id;
                    newCustomerPointHistory.point_amount = point_subtract;
                    newCustomerPointHistory.status = 5;
                    newCustomerPointHistory.trans_date = DateTime.Now;
                    newCustomerPointHistory.point_type = "AVAIABLE";
                    _context.CustomerPointHistorys.Add(newCustomerPointHistory);

                    _context.SaveChanges();
                }
                else
                {
                    if (userPartnerObj.point_avaiable > 0)
                    {
                        userPartnerObj.point_avaiable = 0;
                    }
                    userPartnerObj.point_waiting = 0;
                    userPartnerObj.point_affiliate = 0;
                    userPartnerObj.total_point = userPartnerObj.point_avaiable;
                    _context.SaveChanges();

                    // Tạo bảng PartnerPointHistory
                    var newPointHistory = new PartnerPointHistory();
                    newPointHistory.id = Guid.NewGuid();
                    newPointHistory.order_type = "RECALL_POINT";
                    newPointHistory.partner_id = userPartnerObj.partner_id;
                    newPointHistory.point_amount = point_subtract;
                    newPointHistory.status = 5;
                    newPointHistory.trans_date = DateTime.Now;
                    newPointHistory.point_type = "AVAIABLE";
                    _context.PartnerPointHistorys.Add(newPointHistory);

                    _context.SaveChanges();

                    // Cộng điểm hệ thống
                    var newSystemPointHistory = new SystemPointHistory();
                    newSystemPointHistory.id = Guid.NewGuid();
                    newSystemPointHistory.order_type = "RECALL_POINT";
                    newSystemPointHistory.point_amount = point_subtract;
                    newSystemPointHistory.status = 5;
                    newSystemPointHistory.trans_date = DateTime.Now;
                    newSystemPointHistory.point_type = "AVAIABLE";

                    _context.SystemPointHistorys.Add(newSystemPointHistory);

                    _context.SaveChanges();
                }

                // Gửi email
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
                    string subjectEmail = "[CashPlus] - Thông báo thu hồi điểm";
                    string message = "<p>Xin chào " + (userType == "CUSTOMER" ? userCustomerObj.full_name : userPartnerObj.full_name) + "!<p>";
                    message += "<p>Thông báo Điểm thưởng của bạn nhận được trước đó đã bị thu hồi do vi pham <b>Điều khoản & chính sách sử dụng</b> của CashPlus.</p>";
                    message += "<p>Việc này được thực hiện để đảm bảo tính công bằng cho tất cả người dùng của CashPus.</p>";
                    message += "<br/>";
                    message += "<p>Nếu bạn có bất kỳ câu hỏi hoặc yêu cầu hỗ trợ, vui lòng liên hệ với bộ phận chăm sóc khách hàng của chúng tôi.</p>";
                    message += "<p>Xem thêm điều khoản & chính sách sử dụng tại <a href='" + Consts.WEB_CSPL + "'>đây</a>.</p>";
                    message += "<p>Trân trọng!</p>";
                    message += "<br/>";
                    message += "<p>@2023 ATS Group</p>";

                    _emailSender.SendEmailAsync(mail_to, subjectEmail, message);
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
            return new APIResponse(200);
        }

    }
}
