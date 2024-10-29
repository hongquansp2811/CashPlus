
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
    [Route("api/store/user")]
    [Authorize(Policy = "WebPartnerUser")]
    [ApiController]
    public class PartnerUserController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IPartnerUser _app;
        private readonly ILoggingHelpers _logging;
        public PartnerUserController(IConfiguration configuration, IPartnerUser app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(UserRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.partner_id = userId;
            APIResponse data = _app.getList(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/partner/user/list",
                actions = "Danh sách Nhân viên cửa hàng",
                application = "PARTER APP",
                content = "Danh sách Nhân viên cửa hàng",
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
            APIResponse data = _app.getDetail(id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/partner/user/" + id,
                actions = "Chi tiết Nhân viên cửa hàng",
                application = "PARTER APP",
                content = "Chi tiết Nhân viên cửa hàng",
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
        public JsonResult Create(UserRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.partner_id = userId;
            APIResponse data = _app.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/partner/user/create",
                actions = "Tạo mới Nhân viên cửa hàng",
                application = "PARTER APP",
                content = "Tạo mới Nhân viên cửa hàng",
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
        public JsonResult Update(UserRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.update(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/partner/user/update",
                actions = "Cập nhật Nhân viên cửa hàng",
                application = "PARTER APP",
                content = "Cập nhật Nhân viên cửa hàng",
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
        public JsonResult Delete(DeleteGuidRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.delete(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/partner/user/delete",
                    actions = "Xóa Nhân viên cửa hàng",
                    application = "PARTER APP",
                    content = "Xóa Nhân viên cửa hàng",
                    functions = "Danh mục",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("lock")]
        [HttpPost]
        public JsonResult LockAccount(DeleteGuidRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.lockAccount(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/partner/user/lock",
                    actions = "Khóa Nhân viên cửa hàng",
                    application = "PARTER APP",
                    content = "Khóa Nhân viên cửa hàng",
                    functions = "Danh mục",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("unlock")]
        [HttpPost]
        public JsonResult Unlock(DeleteGuidRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.unlockAccount(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/partner/user/unlock",
                    actions = "Mở khóa Nhân viên cửa hàng",
                    application = "PARTER APP",
                    content = "Mở khóa Nhân viên cửa hàng",
                    functions = "Danh mục",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("changePass")]
        [HttpPost]
        public JsonResult changePass(DeleteGuidRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.changePass(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/partner/user/changePass",
                    actions = "Mở khóa Nhân viên cửa hàng",
                    application = "PARTER APP",
                    content = "Mở khóa Nhân viên cửa hàng",
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
