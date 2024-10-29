
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
    [Route("api/staticpage")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class StaticPageController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IStaticPage _app;
        private readonly ILoggingHelpers _logging;
        private static string functions = "cms_masterdata_staticpage";
        public StaticPageController(IConfiguration configuration, IStaticPage app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(StaticPageRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getList(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/staticpage/list",
                actions = "Danh sách Trang tĩnh",
                application = "WEB ADMIN",
                content = "Danh sách Trang tĩnh",
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
            APIResponse data = _app.getDetail(id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/staticpage/" + id,
                actions = "Chi tiết Trang tĩnh",
                application = "WEB ADMIN",
                content = "Chi tiết Trang tĩnh",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("getByCode/{code}")]
        public JsonResult GetDetailByCode(string code)
        {
                        var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getDetailByCode(code);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/staticpage/getByCode/" + code,
                actions = "Chi tiết Trang tĩnh",
                application = "WEB ADMIN",
                content = "Chi tiết Trang tĩnh",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("create")]
        [HttpPost]
        public JsonResult Create(StaticPageRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
                        var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.Add))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/staticpage/create",
                actions = "Tạo mới Trang tĩnh",
                application = "WEB ADMIN",
                content = "Tạo mới Trang tĩnh",
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
        public JsonResult Update(StaticPageRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
                                    var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.Edit))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.update(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/staticpage/update",
                actions = "Cập nhật Trang tĩnh",
                application = "WEB ADMIN",
                content = "Cập nhật Trang tĩnh",
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
            APIResponse data = _app.delete(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/staticpage/delete",
                    actions = "Xóa Trang tĩnh",
                    application = "WEB ADMIN",
                    content = "Xóa Trang tĩnh",
                    functions = "Danh mục",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        // [AllowAnonymous]
        // [HttpPost("ListApp")]
        // public JsonResult ListApp(StaticPageRequest request)
        // {
        //     APIResponse data = _app.getList(request);

        //     // Ghi log
        //     var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
        //     _logging.insertLogging(new LoggingRequest
        //     {
        //         user_type = Consts.USER_TYPE_WEB_ADMIN,
        //         is_call_api = true,
        //         api_name = "api/staticpage/ListApp",
        //         actions = "Danh sách Trang tĩnh",
        //         application = "WEB ADMIN",
        //         content = "Danh sách Trang tĩnh",
        //         functions = "Danh mục",
        //         is_login = false,
        //         result_logging = data.code == "200" ? "Thành công" : "Thất bại",
        //         user_created = "Anonymous",
        //         IP = remoteIP.ToString()
        //     });
        //     return new JsonResult(data) { StatusCode = 200 };
        // }


        // [AllowAnonymous]
        // [HttpGet("GetDetailApp/{id}")]
        // public JsonResult GetDetailApp(Guid id)
        // {
        //     var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
        //     APIResponse data = _app.getDetail(id);

        //     // Ghi log
        //     var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
        //     _logging.insertLogging(new LoggingRequest
        //     {
        //         user_type = Consts.USER_TYPE_WEB_ADMIN,
        //         is_call_api = true,
        //         api_name = "api/staticpage/" + id,
        //         actions = "Chi tiết Trang tĩnh",
        //         application = "WEB ADMIN",
        //         content = "Chi tiết Trang tĩnh",
        //         functions = "Danh mục",
        //         is_login = false,
        //         result_logging = data.code == "200" ? "Thành công" : "Thất bại",
        //         IP = remoteIP.ToString()
        //     });
        //     return new JsonResult(data) { StatusCode = 200 };
        // }
    }
}
