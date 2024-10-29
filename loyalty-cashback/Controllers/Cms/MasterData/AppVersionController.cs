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
    [Route("api/appversion")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class AppVersionController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppVersion _AppVersion;
        private readonly ILoggingHelpers _logging;
        public AppVersionController(IConfiguration configuration, IAppVersion AppVersion, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._AppVersion = AppVersion;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(AppVersionRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            APIResponse data = _AppVersion.getList(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/appversion/list",
                actions = "Danh sách Phiên bản Mobile",
                application = "WEB ADMIN",
                content = "Danh sách Phiên bản Mobile",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("{id}")]
        public JsonResult GetDetail(int id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _AppVersion.getDetail(id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/appversion/" + id,
                actions = "Chi tiết Phiên bản Mobile",
                application = "WEB ADMIN",
                content = "Chi tiết Phiên bản Mobile",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("create")]
        [HttpPost]
        public JsonResult Create(AppVersionRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _AppVersion.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/appversion/create",
                actions = "Tạo mới Phiên bản Mobile",
                application = "WEB ADMIN",
                content = "Tạo mới Phiên bản Mobile",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("update")]
        [HttpPost]
        public JsonResult Update(AppVersionRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _AppVersion.update(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/appversion/update",
                actions = "Cập nhật Phiên bản Mobile",
                application = "WEB ADMIN",
                content = "Cập nhật Phiên bản Mobile",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("delete")]
        [HttpPost]
        public JsonResult Delete(DeleteRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _AppVersion.delete(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/appversion/delete",
                    actions = "Xóa Phiên bản Mobile",
                    application = "WEB ADMIN",
                    content = "Xóa Phiên bản Mobile",
                    functions = "Danh mục",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }
    }
}
