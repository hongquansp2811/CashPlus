using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Interfaces;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.CloudMessaging;
using System.Security.Claims;
using LOYALTY.PaymentGate;

namespace LOYALTY.Controllers
{
    public class PointDashboardResponse
    {
        public string? name { get; set; }
        public int? years { get; set; }
        public int? month { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public decimal? recall_point { get; set; }
        public decimal? affiliate_point { get; set; }
        public decimal? accumulate_point { get; set; }
    }

    public class StaticSevenDayResponse
    {
        public decimal? total_partners { get; set; }
        public decimal? total_customers { get; set; }
        public decimal? total_accumulate_orders { get; set; }
        public decimal? total_accumulate_partner_points { get; set; }
        public decimal? total_affiliate_points { get; set; }
        public decimal? total_change_orders { get; set; }
        public decimal? total_accumulate_customer_points { get; set; }
        public decimal? discount_for_cashplus { get; set; }
        public decimal? offers_cashback_discounts { get; set; }
    }

    public class NewUserDashboardResponse
    {
        public string? name { get; set; }
        public int? years { get; set; }
        public int? month { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public decimal? total_partners { get; set; }
        public decimal? total_customers { get; set; }
    }

    [Route("api/dashboard")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILoggingHelpers _loggingHelpers;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ICommonFunction _commonFunction;
        private readonly FCMNotification _fCMNotification;
        private readonly BKTransaction _bkTransaction;
        public AdminDashboardController(IDistributedCache distributedCache, ILoggingHelpers iLoggingHelpers, LOYALTYContext context, IEmailSender emailSender, ICommonFunction commonFunction, FCMNotification fCMNotification, BKTransaction bKTransaction)
        {
            _distributedCache = distributedCache;
            _loggingHelpers = iLoggingHelpers;
            _context = context;
            _emailSender = emailSender;
            _commonFunction = commonFunction;
            _fCMNotification = fCMNotification;
            _bkTransaction = bKTransaction;
        }


        // API danh sách cửa hàng vi phạm
        [Route("partnerViolate")]
        [HttpPost]
        public JsonResult PartnerViolateDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
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
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           where p.is_violation == true && p.is_partner_admin == true
                           select new
                           {
                               partner_id = p.partner_id,
                               partner_code = s != null ? s.code : "",
                               partner_name = s != null ? s.name : "",
                               date_created = _commonFunction.convertDateToStringFull(s.date_created),
                               username = p.username,
                               point_avaiable = p.point_avaiable,
                               point_affiliate = p.point_affiliate
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
            var result = new APIResponse(dataResult);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/partnerViolate",
                actions = "Danh sách cửa hàng vi phạm",
                application = "WEB ADMIN",
                content = "Danh sách cửa hàng vi phạm",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }

        // API danh sách cửa hàng vi phạm
        [Route("pushNotificationViolate")]
        [HttpPost]
        public async Task<JsonResult> PushNotificationViolate(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            // Default page_no, page_size

            if (request.ids == null || request.ids.Count == 0)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            var partnerIds = (from u in _context.Users
                              join p in _context.Partners on u.partner_id equals p.id
                              where u.is_partner_admin == true && request.ids.Contains((Guid)u.partner_id) == true
                              select new
                              {
                                  partner_id = u.partner_id,
                                  partner_name = p.name,
                                  device_id = u.device_id,
                                  email = p.email
                              }).ToList();

            var transaction = _context.Database.BeginTransaction();

            try
            {
                for (int i = 0; i < partnerIds.Count; i++)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Đánh giá khách hàng";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = partnerIds[i].partner_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    newNoti1.content = "Điểm thanh toán của tài khoản " + partnerIds[i].partner_name + " đã nợ điểm vượt quá hạn mức cho phép.";
                    newNoti1.system_type = "INFO";
                    newNoti1.reference_id = null;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    if (partnerIds[i].device_id != null)
                    {
                        // Gửi FCM cho Shop
                        await _fCMNotification.SendNotification(partnerIds[i].device_id,
                            "INFO",
                            "Đánh giá khách hàng",
                            "Điểm thanh toán của tài khoản " + partnerIds[i].partner_name + " đã nợ điểm vượt quá hạn mức cho phép.",
                            null);
                    }

                    if (partnerIds[i].email != null)
                    {
                        string subject = "[CashPlus] - Thông báo tài khoản vi phạm";
                        string message = "<p>Xin chào!<p>";
                        message += "<p>Điểm thanh toán của tài khoản " + partnerIds[i].partner_name + " đã nợ điểm vượt quá hạn mức cho phép.</p>";
                        message += "<p>Vui lòng thực hiện nạp thêm điểm thanh toán vào ví cá nhân để không bị gián đoạn giao dịch.</p>";
                        message += "<p>Nếu bạn không thực hiện nạp thêm điểm vào cá nhân, chúng tôi sẽ áp dụng chính sách xử lý vi phạm theo điều khoản sử dụng của hệ thống.</p>";
                        message += "<p><a href='https://cashplus.vn/tin-chinh-sach-bao-mat'>https://cashplus.vn/tin-chinh-sach-bao-mat</a></p>";
                        message += "<p>Trân trọng!</p>";
                        message += "<br/>";
                        message += "<p>@2023 ATS Group</p>";
                        await _emailSender.SendEmailAsync(partnerIds[i].email, subject, message);
                    }
                }
            }
            catch
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            transaction.Commit();
            transaction.Dispose();
            var result = new APIResponse(200);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/partnerViolate",
                actions = "Danh sách cửa hàng vi phạm",
                application = "WEB ADMIN",
                content = "Danh sách cửa hàng vi phạm",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }


        // API thông tin chung
        [Route("common")]
        [HttpPost]
        public JsonResult CommonDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            // Thông số common
            var total_point = _context.Users.Select(x => x.total_point).Sum(x => x.Value);
            var total_system_affiliate_point = _context.SystemPointHistorys.Where(x => x.order_type == "AFFILIATE").Sum(x => x.point_amount);
            var total_recall_point = _context.RecallPointOrders.Sum(x => x.point_value);
            var total_partners = _context.Partners.Where(x => x.is_delete != true).Count();
            var total_customers = _context.Customers.Where(x => x.status == 1).Count();
            var total_system_accumulate_point = _context.SystemPointHistorys.Where(x => x.order_type == "PUSH").Sum(x => x.point_amount);
            GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(Consts.CP_BK_PARTNER_CODE, Consts.private_key);
            var available = balanceObj.Available;
            var dataReturn = new
            {
                total_point = total_point,
                total_system_affiliate_point = total_system_affiliate_point != null ? total_system_affiliate_point : 0,
                total_recall_point = total_recall_point != null ? total_recall_point : 0,
                total_partners = total_partners,
                total_customers = total_customers,
                total_system_accumulate_point = total_system_accumulate_point != null ? total_system_accumulate_point : 0,
                available = available,
            };

            var result = new APIResponse(dataReturn);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/common",
                actions = "Common Dashboard",
                application = "WEB ADMIN",
                content = "Common Dashboard",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }

        // API thống kê
        [Route("analyst")]
        [HttpPost]
        public JsonResult AnalystDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var from_date = _commonFunction.convertStringSortToDate(request.from_date);
            var to_date = _commonFunction.convertStringSortToDate(request.to_date);

            // Thông số common
            var total_partners = _context.Partners.Where(x => x.status == 1 && x.date_created >= from_date && x.date_created <= to_date).Count();
            var total_customers = _context.Customers.Where(x => x.status == 1 && x.date_created >= from_date && x.date_created <= to_date).Count();
            var total_accumulate_order = _context.AccumulatePointOrders.Where(x => x.status == 5 && x.approve_date >= from_date && x.approve_date <= to_date).Count();
            var total_accumulate_point = _context.AccumulatePointOrders.Where(x => x.status == 5 && x.approve_date >= from_date && x.approve_date <= to_date).Sum(x => x.point_partner);
            var total_change_order = _context.ChangePointOrders.Where(x => (x.status == 5 || x.status == 4) && x.date_created >= from_date && x.date_created <= to_date).Count();
            var total_change_point = _context.ChangePointOrders.Where(x => (x.status == 5 || x.status == 4) && x.date_created >= from_date && x.date_created <= to_date).Sum(x => x.point_exchange);

            var dataReturn = new
            {
                total_partners = total_partners,
                total_customers = total_customers,
                total_accumulate_order = total_accumulate_order,
                total_accumulate_point = total_accumulate_point != null ? total_accumulate_point : 0,
                total_change_order = total_change_order,
                total_change_point = total_change_point != null ? total_change_point : 0,
            };

            var result = new APIResponse(dataReturn);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/analyst",
                actions = "Thống kê dashboard",
                application = "WEB ADMIN",
                content = "Thống kê dashboard",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }

        // API báo cáo điểm hệ thống
        [Route("point")]
        [HttpPost]
        public JsonResult PointDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var years = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var dataReturn = new List<PointDashboardResponse>();
            if (request.years != null && request.years != years)
            {
                years = (int)request.years;
                for (int j = 1; j <= 12; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new PointDashboardResponse
                    {
                        name = "Tháng " + monthsString + " - " + years,
                        years = years,
                        month = j,
                        from_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years),
                        to_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years).AddMonths(1).AddDays(-1),
                        accumulate_point = 0,
                        affiliate_point = 0,
                        recall_point = 0
                    });
                }
            }
            else
            {
                for (int j = 1; j <= month; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new PointDashboardResponse
                    {
                        name = "Tháng " + monthsString + " - " + years,
                        years = years,
                        month = j,
                        from_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years),
                        to_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years).AddMonths(1).AddDays(-1),
                        accumulate_point = 0,
                        affiliate_point = 0,
                        recall_point = 0
                    });
                }
            }

            for (int i = 0; i < dataReturn.Count; i++)
            {
                dataReturn[i].recall_point = _context.SystemPointHistorys.Where(x => x.order_type == "RECALL_POINT" && x.trans_date >= dataReturn[i].from_date && x.trans_date <= dataReturn[i].to_date).Sum(x => x.point_amount);
                dataReturn[i].accumulate_point = _context.SystemPointHistorys.Where(x => x.order_type == "PUSH" && x.trans_date >= dataReturn[i].from_date && x.trans_date <= dataReturn[i].to_date).Sum(x => x.point_amount);
                dataReturn[i].affiliate_point = _context.SystemPointHistorys.Where(x => x.order_type == "AFFILIATE" && x.trans_date >= dataReturn[i].from_date && x.trans_date <= dataReturn[i].to_date).Sum(x => x.point_amount);

                if (dataReturn[i].recall_point == null)
                {
                    dataReturn[i].recall_point = 0;
                }

                if (dataReturn[i].accumulate_point == null)
                {
                    dataReturn[i].accumulate_point = 0;
                }

                if (dataReturn[i].affiliate_point == null)
                {
                    dataReturn[i].affiliate_point = 0;
                }
            }

            var result = new APIResponse(dataReturn);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/point",
                actions = "Biểu đồ báo cáo điểm hệ thống",
                application = "WEB ADMIN",
                content = "Biểu đồ báo cáo điểm hệ thống",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }

        // API báo cáo người dùng mới
        [Route("newUser")]
        [HttpPost]
        public JsonResult NewUserDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var years = DateTime.Now.Year;
            var months = DateTime.Now.Month;
            var dataReturn = new List<NewUserDashboardResponse>();
            // Trong trường hợp chọn năm
            if (request.years != null)
            {
                years = (int)request.years;

                for (int j = 1; j <= 12; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new NewUserDashboardResponse
                    {
                        name = "Tháng " + monthsString,
                        years = years,
                        month = j,
                        from_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years),
                        to_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years).AddMonths(1).AddDays(-1),
                        total_partners = 0,
                        total_customers = 0
                    });
                }

            }
            else
            {
                for (int j = 1; j <= months; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new NewUserDashboardResponse
                    {
                        name = "Tháng " + monthsString,
                        years = years,
                        month = j,
                        from_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years),
                        to_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years).AddMonths(1).AddDays(-1),
                        total_partners = 0,
                        total_customers = 0
                    });
                }
            }

            // Cập nhật thông số mới
            for (int i = 0; i < dataReturn.Count; i++)
            {
                dataReturn[i].total_partners = _context.Partners.Where(x => x.date_created >= dataReturn[i].from_date && x.date_created <= dataReturn[i].to_date).Count();
                dataReturn[i].total_customers = _context.Customers.Where(x => x.date_created >= dataReturn[i].from_date && x.date_created <= dataReturn[i].to_date).Count();
            }

            var result = new APIResponse(dataReturn);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/newUser",
                actions = "Biểu đồ báo cáo người dùng mới",
                application = "WEB ADMIN",
                content = "Biểu đồ báo cáo người dùng mới",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }

        // API Báo cáo thống kê 7 ngày gần nhất
        [Route("static")]
        [HttpPost]
        public JsonResult StaticDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var from_date = _commonFunction.convertStringSortToDate(request.from_date).Date;
            var to_date = _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1);

            var dataReturn = new StaticSevenDayResponse();

            dataReturn.total_partners = _context.Partners.Where(x => x.date_created >= from_date && x.date_created <= to_date).Select(x => x.id).Count();
            dataReturn.total_customers = _context.Customers.Where(x => x.date_created >= from_date && x.date_created <= to_date).Select(x => x.id).Count();
            dataReturn.total_accumulate_orders = _context.AccumulatePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5).Select(x => x.id).Count();
            dataReturn.total_accumulate_partner_points = _context.AccumulatePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5).Select(x => x.point_partner).Sum(x => x.Value);
            dataReturn.total_accumulate_customer_points = _context.ChangePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5).Select(x => x.point_exchange).Sum(x => x.Value);
            dataReturn.total_affiliate_points = _context.CustomerPointHistorys.Where(x => x.trans_date >= from_date && x.trans_date <= to_date && x.order_type == "AFF_LV_1").Select(x => x.point_amount).Sum(x => x.Value);
            dataReturn.total_change_orders = _context.ChangePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5).Select(x => x.id).Count();

            var partner_contract = _context.PartnerContracts.Where(x => x.status == 12).ToList();
            var accumulate_orders = _context.AccumulatePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5).ToList();

            var setting = _context.Settingses.First();
            dataReturn.discount_for_cashplus = 0;
            dataReturn.offers_cashback_discounts = 0;
            for (int i = 0; i < accumulate_orders.Count(); i++)
            {
                for (int y = 0; y < partner_contract.Count(); y++)
                {
                    if (accumulate_orders[i].partner_id == partner_contract[y].partner_id)
                    {
                        var AccumulatePointConfig = new AccumulatePointConfig();
                        if (partner_contract[y].is_GENERAL == true || partner_contract[y].is_GENERAL == null)
                        {
                            AccumulatePointConfig = _context.AccumulatePointConfigs.FirstOrDefault(p => p.code == "GENERAL");
                        }
                        else
                        {
                            AccumulatePointConfig = _context.AccumulatePointConfigs.FirstOrDefault(p => p.code == null && p.partner_id == partner_contract[y].partner_id && p.status == 23);
                        }

                        var percent_discount = new AccumulatePointConfigDetail();
                        if (AccumulatePointConfig != null)
                        {
                            percent_discount = _context.AccumulatePointConfigDetails.FirstOrDefault(x => x.name == "Khách hàng" && x.accumulate_point_config_id == AccumulatePointConfig.id);
                        }
                        else
                        {
                            continue;
                        }

                        if (accumulate_orders[i].payment_type == "Cash")
                        {
                            if (accumulate_orders[i].return_type == "Cash")
                            {
                                //(accumulate_orders[i].bill_amount * (partner_contract[y].discount_rate / 100)) = tổng chiết khấu
                                dataReturn.discount_for_cashplus += (accumulate_orders[i].bill_amount * (partner_contract[y].discount_rate / 100)) * ((100 - percent_discount.discount_rate) / 100);
                                break;
                            }
                            else
                            {
                                dataReturn.discount_for_cashplus += accumulate_orders[i].bill_amount * (partner_contract[y].discount_rate / 100);
                                break;
                            }
                        }
                        else
                        {
                            if (accumulate_orders[i].return_type == "Cash")
                            {
                                dataReturn.discount_for_cashplus += (accumulate_orders[i].bill_amount * (partner_contract[y].discount_rate / 100)) * ((100 - percent_discount.discount_rate) / 100);
                                dataReturn.offers_cashback_discounts += ((accumulate_orders[i].bill_amount * (partner_contract[y].discount_rate / 100)) * ((100 - percent_discount.discount_rate) / 100)) * (percent_discount.discount_rate / 100);
                                break;
                            }
                            else
                            {
                                dataReturn.discount_for_cashplus += accumulate_orders[i].bill_amount * (partner_contract[y].discount_rate / 100);
                                //dataReturn.offers_cashback_discounts += (accumulate_orders[i].bill_amount  (partner_contract[y].discount_rate / 100))  (percent_discount.discount_rate / 100);
                                break;
                            }
                        }

                    }
                }
            };


            // Trong trường hợp chọn năm

            var result = new APIResponse(dataReturn);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/static",
                actions = "Biểu đồ báo cáo người dùng mới",
                application = "WEB ADMIN",
                content = "Biểu đồ báo cáo người dùng mới",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }


        [Route("testSchedule")]
        [HttpPost]
        public JsonResult TestSchedule()
        {
            // Xử lý affiliate
            var configSettings = _context.AffiliateConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();

            if (configSettings == null)
            {
                return new JsonResult(new APIResponse("ERROR_1")) { StatusCode = 200 };
            }

            // Tìm ngày gần nhất trước đó 
            // Ngày hôm nay
            var yearNow = DateTime.Now.Year;
            var monthNow = DateTime.Now.Month;
            var dayNow = DateTime.Now.Day;
            var hourNow = DateTime.Now.Hour;
            var minuteNow = DateTime.Now.Minute;

            // Ngày cấu hình
            var dayConfig = configSettings.date_return;
            TimeSpan dateConvert = (TimeSpan)configSettings.hours_return;
            var stringConfig = dateConvert.ToString("hh\\:mm");

            var hourConfig = stringConfig.Split(":")[0];
            var minuteConfig = stringConfig.Split(":")[1];
            var stringDateInMonth = dayConfig.ToString().PadLeft(2, '0') + "/" + monthNow.ToString().PadLeft(2, '0') + "/" + yearNow
                + " " + hourConfig.ToString().PadLeft(2, '0') + ":" + minuteConfig.ToString().PadLeft(2, '0') + ":00";

            var dateInMonth = DateTime.ParseExact(stringDateInMonth, "dd/MM/yyyy HH:mm:ss", null);
            var startInMonth = DateTime.ParseExact("01/" + monthNow.ToString().PadLeft(2, '0') + "/" + yearNow
                + " 00:00:01", "dd/MM/yyyy HH:mm:ss", null);

            // Nếu thời gian cấu hình chưa tới ngày hiện tại thì chưa chạy
            if (dateInMonth < DateTime.Now)
            {
                return new JsonResult(new APIResponse("ERROR_2")) { StatusCode = 200 };
            }

            // Nếu đến thời gian rồi xem đã chạy trong tháng chưa
            var objSchedule = _context.ScheduleJobss.Where(x => x.date_created >= startInMonth).FirstOrDefault();
            if (objSchedule != null)
            {
                return new JsonResult(new APIResponse("ERROR_3")) { StatusCode = 200 };
            }
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // Thông báo
        [Route("readNoti/{id}")]
        [HttpGet]
        public JsonResult readNoti(Guid id)
        {
            var userSystemId = _context.Users.Where(x => x.is_sysadmin == true && x.username == "administrator").Select(x => x.id).FirstOrDefault();

            var data = _context.Notifications.Where(x => x.id == id).FirstOrDefault();
            if (data == null)
            {
                return new JsonResult(new APIResponse("ERROR_ID_NOT_EXISTS")) { StatusCode = 200 };
            }

            try
            {
                var dataNoti = _context.UserNotifications.Where(x => x.user_id == userSystemId && x.notification_id == id).FirstOrDefault();

                if (dataNoti == null)
                {
                    var newDataNoti = new UserNotification();
                    newDataNoti.user_id = userSystemId;
                    newDataNoti.notification_id = id;
                    newDataNoti.date_read = DateTime.Now;

                    _context.UserNotifications.Add(newDataNoti);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        [Route("getListNoti")]
        [HttpPost]
        public JsonResult getList(NotificationRequest request)
        {
            var userSystemId = _context.Users.Where(x => x.is_sysadmin == true && x.username == "administrator").Select(x => x.id).FirstOrDefault();
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
            var lstData = (from p in _context.Notifications
                           join u in _context.UserNotifications.Where(x => x.user_id == userSystemId) on p.id equals u.notification_id into us
                           from u in us.DefaultIfEmpty()
                           where p.user_id == null || p.user_id == userSystemId
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               type_id = p.type_id,
                               tit = p.title,
                               avatar = p.avatar,
                               description = p.description,
                               is_read = u != null ? true : false,
                               date_created = p.date_created != null ? _commonFunction.convertDateToStringFull(p.date_created) : ""
                           });

            if (request.type_id != null)
            {
                lstData = lstData.Where(x => x.type_id == request.type_id);
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
            return new JsonResult(new APIResponse(dataResult)) { StatusCode = 200 };
        }
    }
}
