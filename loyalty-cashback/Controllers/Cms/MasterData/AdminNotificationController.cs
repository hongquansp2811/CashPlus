using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using System.Security.Claims;
using Org.BouncyCastle.Ocsp;

namespace LOYALTY.Controllers
{
    [Route("api/AdminNotification")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class AdminNotificationController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAdminNotification _app;
        private readonly ILoggingHelpers _logging;
        public AdminNotificationController(IConfiguration configuration, IAdminNotification app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("listNotification")]
        [HttpPost]
        public JsonResult getList(NotificationRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.getListNoti(request, Guid.Parse(Consts.USER_ADMIN_ID));
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/AdminNotification/listNotification",
                actions = "Danh sách thông báo",
                application = "WEB ADMIN",
                content = "Danh sách thông báo",
                functions = "Thông báo",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("update")]
        [HttpPost]
        public JsonResult update(NotificationRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            APIResponse data = _app.update(request, Guid.Parse(Consts.USER_ADMIN_ID));
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/AdminNotification/update",
                actions = "Danh sách thông báo",
                application = "WEB ADMIN",
                content = "Danh sách thông báo",
                functions = "Thông báo",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }
    }
}
