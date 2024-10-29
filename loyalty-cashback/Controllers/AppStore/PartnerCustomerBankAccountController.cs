
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
    [Route("api/store/bankaccount")]
    [Authorize(Policy = "WebPartnerUser")]
    [ApiController]
    public class PartnerCustomerBankAccountController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IPartnerBankAccount _app;
        private readonly ILoggingHelpers _logging;
        public PartnerCustomerBankAccountController(IConfiguration configuration, IPartnerBankAccount app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult getList()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getListBankAccount(userId);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/bankaccount/list",
                actions = "Danh sách tài khoản ngân hàng đối tác",
                application = "PARTNER APP",
                content = "Danh sách tài khoản ngân hàng đối tác",
                functions = "PARTNER APP",
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
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/bankaccount/detail/" + id,
                actions = "Chi tiết tài khoản ngân hàng đối tác",
                application = "PARTNER APP",
                content = "Chi tiết tài khoản ngân hàng đối tác",
                functions = "PARTNER APP",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("create")]
        [HttpPost]
        public async Task<JsonResult> Create(CustomerBankAccountRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.user_id = userId;
            APIResponse data = await _app.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/bankaccount/create",
                actions = "Thêm mới tài khoản ngân hàng đối tác",
                application = "PARTNER APP",
                content = "Thêm mới tài khoản ngân hàng đối tác",
                functions = "PARTNER APP",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("update")]
        [HttpPost]
        public JsonResult Update(CustomerBankAccountRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.user_id = userId;
            APIResponse data = _app.update(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/bankaccount/update",
                actions = "Cập nhật tài khoản ngân hàng đối tác",
                application = "PARTNER APP",
                content = "Cập nhật tài khoản ngân hàng đối tác",
                functions = "PARTNER APP",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("delete")]
        [HttpPost]
        public JsonResult Delete(DeleteGuidRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.delete(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/bankaccount/delete",
                actions = "Xóa tài khoản ngân hàng đối tác",
                application = "PARTNER APP",
                content = "Xóa tài khoản ngân hàng đối tác",
                functions = "PARTNER APP",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("bank-detail/{code}")]
        public JsonResult getBankDetail(string code)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.getBankDetail(code);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;

            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/bankaccount/bank-detail/" + code,
                actions = "Chi tiết Ngân hàng",
                application = "WEB ADMIN",
                content = "Chi tiết Ngân hàng",
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
