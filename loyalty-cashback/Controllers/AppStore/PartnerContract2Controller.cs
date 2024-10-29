
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
    [Route("api/store/partner")]
    [Authorize(Policy = "WebPartnerUser")]
    [ApiController]
    public class PartnerContract2Controller : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppPartnerContract _app;
        private readonly IPartner _partnerApp;
        private readonly ILoggingHelpers _logging;
        public PartnerContract2Controller(IConfiguration configuration, IAppPartnerContract app, IPartner partnerApp, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            _partnerApp = partnerApp;
            this._logging = logging;
        }

        [HttpGet("detailPartner")]
        public JsonResult GetDetail()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            APIResponse data = _app.getDetailPartner(userId);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/partner/detailPartner",
                actions = "Chi tiết Cửa hàng",
                application = "PARTER APP",
                content = "Chi tiết Cửa hàng",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("getBalance")]
        public JsonResult GetBalance()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            APIResponse data = _partnerApp.getBalance(userId);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/partner/getBalance",
                actions = "Chi tiết Cửa hàng",
                application = "PARTER APP",
                content = "Chi tiết Cửa hàng",
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
        public JsonResult UpdateStoreInfo(PartnerRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            APIResponse data = _partnerApp.updateInStore(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/partner/update",
                actions = "Cập nhật thông tin cửa hàng",
                application = "PARTER APP",
                content = "Cập nhật thông tin cửa hàng",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }


        [HttpGet("detailContract")]
        public JsonResult GetDetailContract()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            APIResponse data = _app.getDetailPartnerContract(userId);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/store/partner/detailContract",
                actions = "Chi tiết hợp đồng",
                application = "PARTER APP",
                content = "Chi tiết hợp đồng",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListAccumulatePointOrder")]
        [HttpPost]
        public JsonResult getListAccumulatePointOrder(AccumulatePointOrderRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            req.partner_id = userId;
            APIResponse data = _partnerApp.getListAccumulatePointOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/store/partner/getListAccumulatePointOrder",
                    actions = "Danh sách chứng từ tích điểm",
                    application = "PARTNER APP",
                    content = "Danh sách chứng từ tích điểm",
                    functions = "Nghiệp vụ",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListChangePointOrder")]
        [HttpPost]
        public JsonResult getListChangePointOrder(ChangePointOrderRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            req.user_id = userId;
            APIResponse data = _partnerApp.getListChangePointOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/store/partner/getListChangePointOrder",
                    actions = "Danh sách chứng từ đổi điểm",
                    application = "PARTNER APP",
                    content = "Danh sách chứng từ đổi điểm",
                    functions = "Nghiệp vụ",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListPartnerOrder")]
        [HttpPost]
        public JsonResult getListPartnerOrder(PartnerOrderRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            req.partner_id = userId;
            APIResponse data = _partnerApp.getListPartnerOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/store/partner/getListPartnerOrder",
                    actions = "Danh sách chứng từ mua hàng",
                    application = "PARTNER APP",
                    content = "Danh sách chứng từ mua hàng",
                    functions = "Nghiệp vụ",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListAddPointOrder")]
        [HttpPost]
        public JsonResult getListAddPointOrder(AddPointOrderRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            req.partner_id = userId;
            APIResponse data = _partnerApp.getListAddPointOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/store/partner/getListAddPointOrder",
                    actions = "Danh sách nạp điểm",
                    application = "PARTNER APP",
                    content = "Danh sách nạp điểm",
                    functions = "Nghiệp vụ",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListAccumulatePointOrderRating")]
        [HttpPost]
        public JsonResult getListAccumulatePointOrderRating(AccumulatePointOrderRatingRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            req.partner_id = userId;
            APIResponse data = _partnerApp.getListAccumulatePointOrderRating(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/store/partner/getListAccumulatePointOrderRating",
                    actions = "Danh sách đánh giá đơn hàng",
                    application = "PARTNER APP",
                    content = "Danh sách đánh giá đơn hàng",
                    functions = "Nghiệp vụ",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListTeam")]
        [HttpPost]
        public JsonResult getListTeam(PartnerRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);
            req.partner_id = userId;
            APIResponse data = _partnerApp.getListTeam(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "api/store/partner/getListTeam",
                    actions = "Danh sách đội nhóm",
                    application = "PARTNER APP",
                    content = "Danh sách đội nhóm",
                    functions = "Nghiệp vụ",
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
