
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
    [Route("api/app/customer/changepointorder")]
    [Authorize(Policy = "AppUser")]
    [ApiController]
    public class AppChangePointOrderController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppChangePointOrder _app;
        private readonly ILoggingHelpers _logging;
        public AppChangePointOrderController(IConfiguration configuration, IAppChangePointOrder app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(ChangePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.user_id = userId;
            APIResponse data = _app.getList(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/changepointorder/list",
                actions = "Danh sách Chứng từ đổi điểm",
                application = "APP LOYALTY",
                content = "Danh sách Chứng từ đổi điểm",
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
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/changepointorder/" + id,
                actions = "Chi tiết Chứng từ đổi điểm",
                application = "APP LOYALTY",
                content = "Chi tiết Chứng từ đổi điểm",
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
        public JsonResult Create(ChangePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.user_id = userId;
            request.trans_type_id = 1;
            APIResponse data = _app.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/changepointorder/create",
                actions = "Tạo mới Chứng từ đổi điểm",
                application = "APP LOYALTY",
                content = "Tạo mới Chứng từ đổi điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("cancelChangePoint")]
        [HttpPost]
        public JsonResult cancelChangePoint(ChangePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            APIResponse data = _app.cancelChangePoint(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/changepointorder/cancelChangePoint",
                actions = "Hủy Chứng từ đổi điểm",
                application = "APP LOYALTY",
                content = "Hủy Chứng từ đổi điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getExchangePack")]
        [HttpGet]
        public JsonResult getExchangePack()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            APIResponse data = _app.getExchangePack();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/changepointorder/getExchangePack",
                actions = "Lấy danh sách gói đổi điểm",
                application = "APP LOYALTY",
                content = "Lấy danh sách gói đổi điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = "Anonymouse",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }
    }
}
