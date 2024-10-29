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
using LOYALTY.Services;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace LOYALTY.Controllers
{
    [Route("api/app/customer/partnerorder")]
    [Authorize(Policy = "AppUser")]
    [ApiController]
    public class AppPartnerOrderController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppPartnerOrder _app;
        private readonly ILoggingHelpers _logging;
        
        public AppPartnerOrderController(IConfiguration configuration, IAppPartnerOrder app, 
            ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
           
        }

        [Route("list")]
        [HttpPost]
        public JsonResult getList(PartnerOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.customer_id = userId;
            APIResponse data = _app.getList(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/partnerorder/list",
                actions = "Danh sách đơn hàng",
                application = "APP LOYALTY",
                content = "Danh sách đơn hàng",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("detail/{id}")]
        public JsonResult getDetail(Guid id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getDetail(id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/partnerorder/detail/" + id,
                actions = "Chi tiết đơn hàng",
                application = "APP LOYALTY",
                content = "Chi tiết đơn hàng",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("create")]
        [HttpPost]
        public JsonResult Create(PartnerOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.customer_id = userId;
            APIResponse data = _app.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/partnerorder/listPartner",
                actions = "Tạo đơn hàng",
                application = "APP LOYALTY",
                content = "Tạo đơn hàng",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listProductGroup")]
        [HttpPost]
        public JsonResult getListProductGroup(CategoryRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            //request.customer_id = userId;
            APIResponse data = _app.getListProductGroup(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/partnerorder/listProductGroup",
                actions = "Danh sách nhóm sản phẩm từ loại dịch vụ",
                application = "APP LOYALTY",
                content = "Danh sách nhóm sản phẩm từ loại dịch vụ",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listPartner")]
        [HttpPost]
        public JsonResult getListPartner(PartnerMapRequest request)
        {

            //var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            //request.customer_id = userId;
            APIResponse data = _app.getListPartnerTest(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/partnerorder/listPartner",
                actions = "Danh sách đối tác từ nhóm sản phẩm",
                application = "APP LOYALTY",
                content = "Danh sách đối tác từ nhóm sản phẩm",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                //user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

    }
}
