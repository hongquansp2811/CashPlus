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
    [Route("api/blog")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class BlogController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IBlog _app;
        private readonly ILoggingHelpers _logging;

        private static string functions = "cms_masterdata_bloglist_blog";
        public BlogController(IConfiguration configuration, IBlog app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(BlogRequest request)
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
                api_name = "api/blog/list",
                actions = "Danh sách Bài viết tin tức",
                application = "WEB ADMIN",
                content = "Danh sách Bài viết tin tức",
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
                api_name = "api/blog/" + id,
                actions = "Chi tiết Bài viết tin tức",
                application = "WEB ADMIN",
                content = "Chi tiết Bài viết tin tức",
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
        public JsonResult Create(BlogRequest request)
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
                api_name = "api/blog/create",
                actions = "Tạo mới Bài viết tin tức",
                application = "WEB ADMIN",
                content = "Tạo mới Bài viết tin tức",
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
        public JsonResult Update(BlogRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var list_permission = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(list_permission, functions, (int)Enums.ActionType.Add))
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
                api_name = "api/blog/update",
                actions = "Cập nhật Bài viết tin tức",
                application = "WEB ADMIN",
                content = "Cập nhật Bài viết tin tức",
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
                    api_name = "api/blog/delete",
                    actions = "Xóa Bài viết tin tức",
                    application = "WEB ADMIN",
                    content = "Xóa Bài viết tin tức",
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
