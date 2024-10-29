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
    [Route("api/app/customer/blog")]
    [Authorize(Policy = "AppUser")]
    [ApiController]
    public class AppBlogController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppBlog _app;
        private readonly ILoggingHelpers _logging;
        public AppBlogController(IConfiguration configuration, IAppBlog app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("listBlogCategory")]
        [HttpPost]
        public JsonResult getListBlogCategory()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getListCategory();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/blog/listBlogCategory",
                actions = "Danh sách tin tức",
                application = "APP LOYALTY",
                content = "Danh sách tin tức",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listBlog")]
        [HttpPost]
        public JsonResult getList(BlogRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getListBlog(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/blog/listBlog",
                actions = "Danh sách tin tức",
                application = "APP LOYALTY",
                content = "Danh sách tin tức",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("detailBlog/{id}")]
        public JsonResult getDetail(Guid id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getDetailBlog(id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/blog/detailBlog/" + id,
                actions = "Chi tiết tin tức",
                application = "APP LOYALTY",
                content = "Chi tiết tin tức",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }
    }
}
