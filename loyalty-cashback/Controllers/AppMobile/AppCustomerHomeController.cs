using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static LOYALTY.DataAccess.AppCustomerHomeDataAccess;

namespace LOYALTY.Controllers
{
    [Route("api/app/customer/home")]
    [Authorize(Policy = "AppUser")]
    [ApiController]
    public class AppCustomerHomeController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppCustomerHome _app;
        private readonly ICustomer _cusApp;
        private readonly ILoggingHelpers _logging;
        public AppCustomerHomeController(IConfiguration configuration, IAppCustomerHome app, ICustomer cusApp, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            _cusApp = cusApp;
            this._logging = logging;
        }

        [HttpGet("homeInfo")]
        public JsonResult getHomeInfo()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getHomeInfo(userId);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/home/homeInfo",
                actions = "Lấy thông tin chung",
                application = "APP LOYALTY",
                content = "Lấy thông tin chung",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("generalInfo")]
        public JsonResult getHomeGeneralInfo()
        {
            APIResponse data = _app.getHomeGeneralInfo();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/home/generalInfo",
                actions = "Lấy thông tin chung",
                application = "APP LOYALTY",
                content = "Lấy thông tin chung",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("getNotiNotRead")]
        public JsonResult getNotiNotRead()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getTotalNotificationNotRead(userId);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/home/getNotiNotRead",
                actions = "Lấy số thông báo chưa đọc",
                application = "APP LOYALTY",
                content = "Lấy số thông báo chưa đọc",
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
        public async Task<JsonResult> GetListPartner(PartnerRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = await _app.getListPartner(request, username != null ? username.Value : "Anonymous");

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/listPartner",
                actions = "Danh sách đối tác",
                application = "APP LOYALTY",
                content = "Danh sách đối tác",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("likePartner")]
        [HttpPost]
        public JsonResult LikePartner(PartnerFavourite request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.likePartner(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/likePartner",
                actions = "Like đối tác yêu thích",
                application = "APP LOYALTY",
                content = "Like đối tác yêu thích",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("dislikePartner")]
        [HttpPost]
        public JsonResult DislikePartner(PartnerFavourite request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.dislikePartner(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/dislikePartner",
                actions = "Dislike đối tác yêu thích",
                application = "APP LOYALTY",
                content = "Dislike đối tác yêu thích",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listPartnerFavourite")]
        [HttpPost]
        public JsonResult GetListPartnerFavourite(PartnerRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getListPartnerFavourite(request, username.Value);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/listPartnerFavourite",
                actions = "Danh sách đối tác yêu thích",
                application = "APP LOYALTY",
                content = "Danh sách đối tác yêu thích",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listRating")]
        [HttpPost]
        public JsonResult GetListRating(AccumulatePointOrderRatingRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getListRating(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/listRating",
                actions = "Danh sách đánh giá đối tác",
                application = "APP LOYALTY",
                content = "Danh sách đánh giá đối tác",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getPointByTime")]
        [HttpPost]
        public JsonResult getPointByTime(reqPoint request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.customer_id = userId;

            APIResponse data = _app.getPointByTime(request);

            //Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/getPointByTime",
                actions = "Điểm hoàn khách hàng",
                application = "APP LOYALTY",
                content = "Điểm hoàn khách hàng",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getSuggestSearch")]
        [HttpPost]
        public JsonResult getSuggestSearch(PartnerRequest request)
        {
            APIResponse data = _app.getSuggestSearch(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/getSuggestSearch",
                actions = "Gợi nhắc tìm kiếm",
                application = "APP LOYALTY",
                content = "Gợi nhắc tìm kiếm",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("productGroup/{partner_id}")]
        public JsonResult getHomeInfo(Guid partner_id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            //var userId = Guid.Parse(user2.Value);

            APIResponse data = _app.getListProductGroup(partner_id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/home/productGroup/" + partner_id,
                actions = "Danh sách nhóm sản phẩm",
                application = "APP LOYALTY",
                content = "Danh sách nhóm sản phẩm",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("listProduct")]
        [HttpPost]
        public JsonResult GetListProduct(ProductRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = user2 != null ? Guid.Parse(user2.Value) : Guid.NewGuid();
            request.customer_id = userId;
            APIResponse data = _app.getListProduct(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/listProduct",
                actions = "Danh sách sản phẩm",
                application = "APP LOYALTY",
                content = "Danh sách sản phẩm",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("product/{product_id}")]
        public JsonResult getDetailProduct(Guid product_id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = user2 != null ? Guid.Parse(user2.Value) : Guid.NewGuid();

            APIResponse data = _app.getDetailProduct(product_id, userId);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/home/product/" + product_id,
                actions = "Chi tiết sản phấm",
                application = "APP LOYALTY",
                content = "Chi tiết sản phấm",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListTeam")]
        [HttpPost]
        public JsonResult getListTeam(CustomerRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var userId = Guid.Parse(user2.Value);

            request.customer_id = userId;
            APIResponse data = _cusApp.getListTeam(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/app/customer/home/getListTeam",
                actions = "Lấy danh sách đội nhóm",
                application = "APP LOYALTY",
                content = "Lấy danh sách đội nhóm",
                functions = "APP LOYTALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("getVersionByPlatform/{platform}")]
        public JsonResult getVersionByPlatform(string platform)
        {

            APIResponse data = _app.getVersionByPlatform(platform);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/getVersionByPlatform/" + platform,
                actions = "Lấy thông tin version mới nhất",
                application = "APP DCVF",
                content = "Lấy thông tin version mới nhất",
                functions = "Trang chủ",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = "Anymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        //[Route("listPartner")]
        //[HttpPost]
        //public async Task<JsonResult> GetListPartnerByLocation(PartnerRequest request)
        //{
        //    var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
        //    //var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
        //    //var userId = Guid.Parse(user2.Value);

        //    APIResponse data = await _app.getListPartner(request, username != null ? username.Value : "Anonymous");

        //    // Ghi log
        //    var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
        //    await _logging.insertLogging(new LoggingRequest
        //    {
        //        user_type = Consts.USER_TYPE_CUSTOMER,
        //        is_call_api = true,
        //        api_name = "api/app/customer/home/listPartner",
        //        actions = "Danh sách đối tác",
        //        application = "APP LOYALTY",
        //        content = "Danh sách đối tác",
        //        functions = "APP LOYALTY",
        //        is_login = false,
        //        result_logging = data.code == "200" ? "Thành công" : "Thất bại",
        //        user_created = username != null ? username.Value : "Anonymous",
        //        IP = remoteIP.ToString()
        //    });
        //    return new JsonResult(data) { StatusCode = 200 };
        //}

        [Route("listPartnerV2")]
        [HttpPost]
        public async Task<JsonResult> GetListPartnerV2(PartnerFilterRequest request)
        {
            var username = User.Claims.FirstOrDefault(p => p.Type.Equals(ClaimTypes.Name));
            APIResponse data = await _app.getListPartnerV2(request, username != null ? username.Value : "Anonymous");

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/customer/home/listPartnerV2",
                actions = "Danh sách đối tác",
                application = "APP LOYALTY",
                content = "Danh sách đối tác",
                functions = "APP LOYALTY",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username != null ? username.Value : "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }
    }
}
