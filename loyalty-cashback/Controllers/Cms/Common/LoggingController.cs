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

namespace LOYALTY.Controllers
{
    [Route("api/logging")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class LoggingController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ILogging _loggingData;
        private readonly ILoggingHelpers _logging;

        private static string listlogin = "cms_logging_loginlogging";
        private static string listaction = "cms_logging_actionlogging";
        private static string listAPI = "cms_logging_apilogging";

        public LoggingController(IConfiguration configuration, ILogging loggingData, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._loggingData = loggingData;
            this._logging = logging;
        }

        [Route("listLogin")]
        [HttpPost]
        public JsonResult getListLogIn(FilterLoggingRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, listlogin, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _loggingData.getListLogIn(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/logging/listLogin",
                actions = "Danh sách Log Đăng nhập",
                application = "WEB ADMIN",
                content = "Danh sách Log Đăng nhập",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listAction")]
        [HttpPost]
        public JsonResult getListAction(FilterLoggingRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, listaction, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }

            APIResponse data = _loggingData.getListAction(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/logging/listLogin",
                actions = "Danh sách Log Đăng nhập",
                application = "WEB ADMIN",
                content = "Danh sách Log Đăng nhập",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listCallApi")]
        [HttpPost]
        public JsonResult getListCallApi(FilterLoggingRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, listAPI, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }

            APIResponse data = _loggingData.getListCallApi(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/logging/listLogin",
                actions = "Danh sách Log call API",
                application = "WEB ADMIN",
                content = "Danh sách Log call API",
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
