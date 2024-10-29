
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
    [Route("api/app/customer/accumulatepointorder")]
    [Authorize(Policy = "AppUser")]
    [ApiController]
    public class AppAccumulatePointOrderController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppAccumulatePointOrder _app;
        private readonly IAccumulatePointOrderComplain _complain;
        private readonly ILoggingHelpers _logging;
        public AppAccumulatePointOrderController(IConfiguration configuration, IAppAccumulatePointOrder app, IAccumulatePointOrderComplain complain, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            _complain = complain;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(AccumulatePointOrderRequest request)
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
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/list",
                actions = "Danh sách Chứng từ tích điểm",
                application = "APP LOYALTY",
                content = "Danh sách Chứng từ tích điểm",
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
                api_name = "api/app/customer/accumulatepointorder/" + id,
                actions = "Chi tiết Chứng từ tích điểm",
                application = "APP LOYALTY",
                content = "Chi tiết Chứng từ tích điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }


        [HttpGet("getPartnerDetailByQR/{partner_id}")]
        public JsonResult getPartnerDetailByQR(Guid partner_id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.getPartnerDetailByQR(partner_id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/getPartnerDetailByQR/" + partner_id,
                actions = "Lấy thông tin cửa hàng từ QR Code",
                application = "APP LOYALTY",
                content = "Lấy thông tin cửa hàng từ QR Code",
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
        public JsonResult Create(AccumulatePointOrderRequest request)
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
                api_name = "api/app/customer/accumulatepointorder/create",
                actions = "Tạo mới Chứng từ tích điểm",
                application = "APP LOYALTY",
                content = "Tạo mới Chứng từ tích điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("updateOrder")]
        [HttpPost]
        public JsonResult updateOrder(AccumulatePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.customer_id = userId;
            APIResponse data = _app.updateOrder(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/updateOrder",
                actions = "Cập nhật Chứng từ tích điểm",
                application = "APP LOYALTY",
                content = "Cập nhật Chứng từ tích điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("createPaymentLink")]
        [HttpPost]
        public async Task<JsonResult> CreatePaymentLink(AccumulatePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            APIResponse data = await _app.createPaymentLink(request);

            //Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/createPaymentLink",
                actions = "Tạo link thanh toán Chứng từ tích điểm",
                application = "APP LOYALTY",
                content = "Tạo link thanh toán Chứng từ tích điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("createRating")]
        [HttpPost]
        public JsonResult CreateRating(AccumulatePointOrderRatingRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.customer_id = userId;
            APIResponse data = _app.createRating(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/createRating",
                actions = "Tạo mới Đánh giá tích điểm",
                application = "APP LOYALTY",
                content = "Tạo mới Đánh giá tích điểm",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listComplain")]
        [HttpPost]
        public JsonResult GetListComplain(AccumulatePointOrderComplainRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.user_created = username.Value;
            APIResponse data = _complain.getList(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/listComplain",
                actions = "Danh sách Khiếu nại",
                application = "APP LOYALTY",
                content = "Danh sách Khiếu nại",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("createComplain")]
        [HttpPost]
        public JsonResult CreateComplain(AccumulatePointOrderComplainRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            APIResponse data = _complain.create(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/createComplain",
                actions = "Tạo mới Khiếu nại",
                application = "APP LOYALTY",
                content = "Tạo mới Khiếu nại",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("GetDetail")]
        [HttpPost]
        public JsonResult GetDetail(AccumulatePointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.QR(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/getPartnerDetailByQR/GetDetail",
                actions = "Lấy thông tin cửa hàng từ QR Code",
                application = "APP LOYALTY",
                content = "Lấy thông tin cửa hàng từ QR Code",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getByTransNo/{trans_no}")]
        [HttpGet]
        public JsonResult getByTransNo(string? trans_no)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.getByTransNo(trans_no);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/accumulatepointorder/getPartnerDetailByQR/GetDetail",
                actions = "Lấy thông tin cửa hàng từ QR Code",
                application = "APP LOYALTY",
                content = "Lấy thông tin cửa hàng từ QR Code",
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
