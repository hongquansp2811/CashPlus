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
    [Route("api/action")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class ActionController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAction _Action;
        private readonly ILoggingHelpers _logging;
        private static string functions = "cms_authen_action";
        public ActionController(IConfiguration configuration, IAction Action, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._Action = Action;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(ActionRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }

            APIResponse data = _Action.getList(request);


            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/action/list",
                actions = "Danh sách Hành động",
                application = "WEB ADMIN",
                content = "Danh sách Hành động",
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
            var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _Action.getDetail(id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/action/" + id,
                actions = "Chi tiết Hành động",
                application = "WEB ADMIN",
                content = "Chi tiết Hành động",
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
        public JsonResult Create(Action1 request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.Add))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _Action.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/action/create",
                actions = "Tạo mới Hành động",
                application = "WEB ADMIN",
                content = "Tạo mới Hành động",
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
        public JsonResult Update(Action1 request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.Edit))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _Action.update(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/action/update",
                actions = "Cập nhật Hành động",
                application = "WEB ADMIN",
                content = "Cập nhật Hành động",
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
            var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.Delete))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _Action.delete(req);

            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/action/delete",
                    actions = "Xóa Hành động",
                    application = "WEB ADMIN",
                    content = "Xóa Hành động",
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
