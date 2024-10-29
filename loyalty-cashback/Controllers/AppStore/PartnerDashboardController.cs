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
using System.Security.Claims;
using LOYALTY.PaymentGate;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Asn1.Ocsp;

namespace LOYALTY.Controllers
{
    public class PartnerPointDashboardResponse
    {
        public string? name { get; set; }
        public int? years { get; set; }
        public int? month { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public decimal? point_partner { get; set; }
        public decimal? point_affiliate { get; set; }
        public decimal? point_add { get; set; }
    }
    public class PartnerRevenueDashboardResponse
    {
        public string? name { get; set; }
        public int? years { get; set; }
        public int? month { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public decimal? orders { get; set; }
        public decimal? total_discount { get; set; }
        public decimal? total_revenue { get; set; }
    }

    public class PartnerStaticDashboardResponse
    {
        public decimal? total_accumulate_orders { get; set; }
        public decimal? total_customers { get; set; }
        public decimal? total_revenue { get; set; }
        public decimal? total_affiliate_points { get; set; }
        public decimal? total_affiliate_users { get; set; }
        public decimal? total_discount { get; set; }
        public decimal? discount_for_cashplus { get; set; }
        public decimal? discount_for_user { get; set; }
    }

    [Route("api/store/dashboard")]
    [Authorize(Policy = "WebPartnerUser")]
    [ApiController]
    public class PartnerDashboardController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILoggingHelpers _loggingHelpers;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ICommonFunction _commonFunction;
        private readonly BKTransaction _bkTransaction;
        public PartnerDashboardController(IDistributedCache distributedCache, ILoggingHelpers iLoggingHelpers, LOYALTYContext context, IEmailSender emailSender, ICommonFunction commonFunction, BKTransaction bKTransaction)
        {
            _distributedCache = distributedCache;
            _loggingHelpers = iLoggingHelpers;
            _context = context;
            _emailSender = emailSender;
            _commonFunction = commonFunction;
            _bkTransaction = bKTransaction;
        }

        // API Thông tin chung
        [Route("common")]
        [HttpPost]
        public JsonResult CommonDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerId = Guid.Parse(user2.Value);


            var partnerObj = _context.Partners.Where(x => x.id == partnerId).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult("ERROR_PARTNER_NOT_FOUND");
            }

            // long amount_balance = 0;
            // var erorr = new object();
            // if (partnerObj.bk_partner_code != null)
            // {

            //     GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

            //     amount_balance = balanceObj.Available;
            //     erorr = balanceObj;   
            // } 

            var returnData = (from p in _context.Users
                              where p.is_partner_admin == true && p.partner_id == partnerId
                              select new
                              {
                                  partner_id = p.partner_id,
                                  point_avaiable = p.point_avaiable,
                                  point_waiting = p.point_waiting,
                                  point_affiliate = p.point_affiliate,
                                  total_point = p.total_point,
                                  total_products = _context.Products.Where(x => x.partner_id == partnerId).Count(),
                                  total_employees = _context.Users.Where(x => x.partner_id == partnerId && x.is_delete != true && x.is_partner == true).Count(),
                              }).FirstOrDefault();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/dashboarc/common",
                actions = "Thông tin chung",
                application = "PARTER APP",
                content = "Thông tin chung",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(returnData)) { StatusCode = 200 };
        }


        // API báo cáo điểm hệ thống
        [Route("point")]
        [HttpPost]
        public JsonResult PointDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerId = Guid.Parse(user2.Value);
            var years = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var dataReturn = new List<PartnerPointDashboardResponse>();

            if (request.years != null && request.years != years)
            {
                years = (int)request.years;
                for (int j = 1; j <= 12; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new PartnerPointDashboardResponse
                    {
                        name = "Tháng " + monthsString + " - " + years,
                        years = years,
                        month = j,
                        from_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years),
                        to_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years).AddMonths(1).AddDays(-1),
                        point_add = 0,
                        point_affiliate = 0,
                        point_partner = 0
                    });
                }
            }
            else
            {
                for (int j = 1; j <= month; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new PartnerPointDashboardResponse
                    {
                        name = "Tháng " + monthsString + " - " + years,
                        years = years,
                        month = j,
                        from_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years),
                        to_date = _commonFunction.convertStringSortToDate("01/" + monthsString + "/" + years).AddMonths(1).AddDays(-1),
                        point_add = 0,
                        point_affiliate = 0,
                        point_partner = 0
                    });
                }
            }

            for (int i = 0; i < dataReturn.Count; i++)
            {
                dataReturn[i].point_add = _context.PartnerPointHistorys.Where(x => x.order_type == "ADD_POINT" && x.trans_date >= dataReturn[i].from_date && x.trans_date <= dataReturn[i].to_date && x.partner_id == partnerId).Sum(x => x.point_amount);
                dataReturn[i].point_affiliate = _context.PartnerPointHistorys.Where(x => x.order_type.Contains("AFF") && x.trans_date >= dataReturn[i].from_date && x.trans_date <= dataReturn[i].to_date && x.partner_id == partnerId).Sum(x => x.point_amount);
                dataReturn[i].point_partner = _context.AccumulatePointOrders.Where(x => x.partner_id == partnerId && x.date_created >= dataReturn[i].from_date && x.date_created <= dataReturn[i].to_date && x.partner_id == partnerId).Sum(x => x.point_partner);

                if (dataReturn[i].point_add == null)
                {
                    dataReturn[i].point_add = 0;
                }

                if (dataReturn[i].point_affiliate == null)
                {
                    dataReturn[i].point_affiliate = 0;
                }

                if (dataReturn[i].point_partner == null)
                {
                    dataReturn[i].point_partner = 0;
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

        // API Báo cáo thống kê 7 ngày gần nhất
        [Route("static")]
        [HttpPost]
        public JsonResult StaticDashboard(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            //var partnerId = _context.Users.Where(x => x.username == username.Value && x.is_partner == true).Select(x => x.partner_id).FirstOrDefault();
            //if (partnerId == null)
            //{
            //    return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            //}
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerId = Guid.Parse(user2.Value);
            var from_date = _commonFunction.convertStringSortToDate(request.from_date).Date;
            var to_date = _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1);

            var dataReturn = new PartnerStaticDashboardResponse();

            dataReturn.total_accumulate_orders = _context.AccumulatePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5 && x.partner_id == partnerId).Select(x => x.id).Count();
            dataReturn.total_customers = _context.AccumulatePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5 && x.partner_id == partnerId).Select(x => x.customer_id).Distinct().Count();
            dataReturn.total_revenue = _context.AccumulatePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5 && x.partner_id == partnerId).Select(x => x.bill_amount).Sum(x => x.Value);
            dataReturn.total_affiliate_points = _context.PartnerPointHistorys.Where(x => x.trans_date >= from_date && x.trans_date <= to_date && x.order_type == "AFF_LV_1" && x.partner_id == partnerId).Select(x => x.point_amount).Sum(x => x.Value);
            dataReturn.total_affiliate_users = _context.Users.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.share_person_id == partnerId).Select(x => x.id).Count();

            var partner_contract = _context.PartnerContracts.FirstOrDefault(x => x.partner_id == partnerId && x.status == 12);
            if (partner_contract == null)
            {
                var result2 = new APIResponse("ERROR_CODE_MISSING");
                return new JsonResult(result2) { StatusCode = 400 };
            }
            var AccumulatePointConfig = new AccumulatePointConfig();
            if (partner_contract.is_GENERAL == true || partner_contract.is_GENERAL == null)
            {
                AccumulatePointConfig = _context.AccumulatePointConfigs.FirstOrDefault(p => p.code == "GENERAL");
            }
            else
            {
                AccumulatePointConfig = _context.AccumulatePointConfigs.FirstOrDefault(p => p.code == null && p.partner_id == partnerId && p.status == 23);
            }
            if(AccumulatePointConfig != null){
                var accumulate_orders = _context.AccumulatePointOrders.Where(x => x.date_created >= from_date && x.date_created <= to_date && x.status == 5 && x.partner_id == partnerId).ToList();
            var percent_discount = _context.AccumulatePointConfigDetails.FirstOrDefault(x => x.name == "Khách hàng" && x.accumulate_point_config_id == AccumulatePointConfig.id);
            var setting = _context.Settingses.First();
            dataReturn.total_discount = 0;
            dataReturn.discount_for_cashplus = 0;
            dataReturn.discount_for_user = 0;
            for (int i = 0; i < accumulate_orders.Count(); i++)
            {

                if (accumulate_orders[i].payment_type == "Cash")
                {
                    if (accumulate_orders[i].return_type == "Cash")//TH tiền mặt hoàn tiền
                    {
                        //(accumulate_orders[i].bill_amount * (partner_contract[y].discount_rate / 100)) = tổng chiết khấu
                        dataReturn.discount_for_cashplus += (accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100)) * ((100 - percent_discount.discount_rate) / 100);
                        dataReturn.discount_for_user += (accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100)) * (percent_discount.discount_rate / 100);
                        dataReturn.total_discount += accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100);

                    }
                    else//TH tiền mặt tích điểm
                    {
                        dataReturn.discount_for_cashplus += accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100);
                        dataReturn.total_discount += accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100);

                    }
                }
                else
                {
                    if (accumulate_orders[i].return_type == "Cash")//TH thanh toán online hoàn tiền
                    {
                        dataReturn.discount_for_cashplus += (accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100)) * ((100 - percent_discount.discount_rate) / 100);
                        dataReturn.discount_for_user += (accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100)) * (percent_discount.discount_rate / 100);
                        dataReturn.total_discount += accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100);

                    }
                    else//TH thanh toán online tích điểm
                    {
                        dataReturn.discount_for_cashplus += accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100);
                        dataReturn.total_discount += accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100);

                    }
                }
            }
            };

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


        // API Khách hàng thân thiết
        [Route("topCustomer")]
        [HttpGet]
        public JsonResult topCustomer()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            var partnerId = _context.Users.Where(x => x.username == username.Value && x.is_partner == true).Select(x => x.partner_id).FirstOrDefault();
            if (partnerId == null)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            var dataReturn = (from p in _context.Customers
                              select new
                              {
                                  avatar = p.avatar,
                                  id = p.id,
                                  username = p.phone,
                                  total_bill = _context.AccumulatePointOrders.Where(x => x.customer_id == p.id && x.partner_id == partnerId).Select(x => x.id).Count(),
                                  total_amount = _context.AccumulatePointOrders.Where(x => x.customer_id == p.id && x.partner_id == partnerId).Select(x => x.bill_amount).Sum(x => x.Value),
                              }).Where(x => x.total_bill > 0).OrderByDescending(x => x.total_bill).Take(5).ToList();
            var result = new APIResponse(dataReturn);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/topCustomer",
                actions = "Báo cáo khách hàng thân thiết",
                application = "WEB ADMIN",
                content = "Báo cáo khách hàng thân thiết",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }

        // API Tài khoản liên kết
        [Route("affiUser")]
        [HttpGet]
        public JsonResult affiUser()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerId = Guid.Parse(user2.Value);

            //var partnerId = _context.Users.Where(x => x.username == username.Value && x.is_partner == true).Select(x => x.partner_id).FirstOrDefault();

            var userObj = _context.Users.Where(x => x.partner_id == partnerId && x.is_partner_admin == true).FirstOrDefault();

            if (userObj == null)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            var dataReturn = (from p in _context.Users
                              join a in _context.Customers on p.customer_id equals a.id
                              where p.share_person_id == userObj.id
                              select new
                              {
                                  avatar = a.avatar,
                                  id = p.id,
                                  username = p.username,
                                  phone = p.username,
                                  total_point_accumulate = (from tp in _context.PartnerPointHistorys
                                                            where tp.order_type == "AFF_LV_1" && tp.source_id == p.customer_id
                                                            select new
                                                            {
                                                                point_amount = tp.point_amount
                                                            }).Sum(x => x.point_amount)
                              }).OrderByDescending(x => x.total_point_accumulate).Take(5).ToList();
            var result = new APIResponse(dataReturn);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/dashboard/topCustomer",
                actions = "Báo cáo khách hàng thân thiết",
                application = "WEB ADMIN",
                content = "Báo cáo khách hàng thân thiết",
                functions = "Tổng quan",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }


        // API báo cáo doanh thu 
        [Route("point2")]
        [HttpPost]
        public JsonResult PointDashboard2(ReportRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerId = Guid.Parse(user2.Value);
            var years = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var dataReturn = new List<PartnerRevenueDashboardResponse>();

            if (request.years != null && request.years != years)
            {
                years = (int)request.years;
                for (int j = 1; j <= 12; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new PartnerRevenueDashboardResponse
                    {
                        name = monthsString + " - " + years,
                        years = years,
                        month = j,
                        from_date = new DateTime(years, j, 1, 0, 0, 0),
                        to_date = new DateTime(years, j, DateTime.DaysInMonth(years, j)),
                        orders = 0,
                        total_discount = 0,
                        total_revenue = 0,
                    });
                }
            }
            else
            {
                for (int j = 1; j <= month; j++)
                {
                    var monthsString = j.ToString().PadLeft(2, '0');
                    dataReturn.Add(new PartnerRevenueDashboardResponse
                    {
                        name = monthsString + " - " + years,
                        years = years,
                        month = j,
                        from_date = new DateTime(years, j, 1, 0, 0, 0),
                        to_date = new DateTime(years, j, DateTime.DaysInMonth(years, j)),
                        orders = 0,
                        total_discount = 0,
                        total_revenue = 0,
                    });
                }
            }

            for (int i = 0; i < dataReturn.Count; i++)
            {

                dataReturn[i].orders = _context.AccumulatePointOrders.Where(x => x.date_created >= dataReturn[i].from_date && x.date_created <= dataReturn[i].to_date && x.status == 5 && x.partner_id == partnerId).Select(x => x.id).Count();
                dataReturn[i].total_revenue = _context.AccumulatePointOrders.Where(x => x.date_created >= dataReturn[i].from_date && x.date_created <= dataReturn[i].to_date && x.status == 5 && x.partner_id == partnerId).Select(x => x.bill_amount).Sum(x => x.Value);

                var accumulate_orders = _context.AccumulatePointOrders.Where(x => x.date_created >= dataReturn[i].from_date && x.date_created <= dataReturn[i].to_date && x.status == 5 && x.partner_id == partnerId).ToList();
                //var percent_discount = _context.AccumulatePointConfigDetails.FirstOrDefault(x => x.name == "Khách hàng");
                var partner_contract = _context.PartnerContracts.FirstOrDefault(x => x.partner_id == partnerId && x.status == 12);
                var setting = _context.Settingses.First();
                if (partner_contract == null)
                {
                    var result2 = new APIResponse("ERROR_CODE_MISSING");
                    return new JsonResult(result2) { StatusCode = 400 };
                }
                for (int y = 0; y < accumulate_orders.Count(); y++)
                {
                    //dataReturn[i].total_discount += accumulate_orders[i].bill_amount * (partner_contract.discount_rate / 100);
                    if (accumulate_orders[i].payment_type == "Cash")
                    {
                        if (accumulate_orders[i].return_type == "Cash")//TH tiền mặt hoàn tiền
                        {
                            dataReturn[i].total_discount += accumulate_orders[y].bill_amount * (partner_contract.discount_rate / 100);

                        }
                        else//TH tiền mặt tích điểm
                        {
                            dataReturn[i].total_discount += accumulate_orders[y].bill_amount * (partner_contract.discount_rate / 100);

                        }
                    }
                    else
                    {
                        if (accumulate_orders[i].return_type == "Cash")//TH thanh toán online hoàn tiền
                        {
                            dataReturn[i].total_discount += accumulate_orders[y].bill_amount * (partner_contract.discount_rate / 100);

                        }
                        else//TH thanh toán online tích điểm
                        {
                            dataReturn[i].total_discount += accumulate_orders[y].bill_amount * (partner_contract.discount_rate / 100);
                        }
                    }
                };

                if (dataReturn[i].orders == null)
                {
                    dataReturn[i].orders = 0;
                }

                if (dataReturn[i].total_revenue == null)
                {
                    dataReturn[i].total_revenue = 0;
                }

                if (dataReturn[i].total_discount == null)
                {
                    dataReturn[i].total_discount = 0;
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

    }
}
