using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.Interfaces;
using LOYALTY.DataAccess;
using LOYALTY.PaymentGate;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LOYALTY.Controllers
{
    [Route("api/baokimtransaction")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class AdminBaoKimTransactionController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ICommon _common;
        private readonly ILoggingHelpers _logging;
        private readonly LOYALTYContext _context;
        private readonly IAdminBaoKimTransaction _app;
        private readonly IEmailSender _emailSender;
        private static JsonSerializerOptions option;
        //private readonly SysTransDataAccess _sysTransDataAccess;
        public AdminBaoKimTransactionController(IConfiguration configuration, ICommon common, ILoggingHelpers logging, LOYALTYContext context, IAdminBaoKimTransaction app, IEmailSender emailSender)
        {
            _configuration = configuration;
            _common = common;
            this._logging = logging;
            _context = context;
            _app = app;
            _emailSender = emailSender;
            //_sysTransDataAccess = sysTransDataAccess;
            option = new JsonSerializerOptions { WriteIndented = true };
        }

        [Route("listBKTransaction")]
        [HttpPost]
        public JsonResult GetList(AccumulatePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            APIResponse data = _app.getListBKTransaction(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/baokimtransaction/listBKTransaction",
                actions = "Danh sách Giao dịch chi tiền",
                application = "WEB ADMIN",
                content = "Danh sách Giao dịch chi tiền",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listPartnerBK")]
        [HttpPost]
        public JsonResult GetListPartnerBK(AccumulatePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            APIResponse data = _app.getListPartnerBK(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/baokimtransaction/listPartnerBK",
                actions = "Danh sách đối tác thu phí",
                application = "WEB ADMIN",
                content = "Danh sách đối tác thu phí",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("paymentCashPlus")]
        [HttpPost]
        public JsonResult PaymentCashPlus(AccumulatePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            APIResponse data = _app.paymentCashPlus(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/baokimtransaction/paymentCashPlus",
                actions = "Thu phí đối tác",
                application = "WEB ADMIN",
                content = "Thu phí đối tác",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("{id}")]
        public JsonResult GetDetail(Guid id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.getDetailBKTransaction(id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/baokimtransaction/" + id,
                actions = "Chi tiết Giao dịch chi tiền",
                application = "WEB ADMIN",
                content = "Chi tiết Giao dịch chi tiền",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

    }
}
