﻿
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
    [Route("api/affiliateconfig")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class AffiliateConfigController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAffiliateConfig _app;
        private readonly ILoggingHelpers _logging;

        private static string functionCode = "cms_config_affiliateconfig";
        public AffiliateConfigController(IConfiguration configuration, IAffiliateConfig app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(AffiliateConfigRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
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
                api_name = "api/affiliateconfig/list",
                actions = "Danh sách Cấu hình Affiliate",
                application = "WEB ADMIN",
                content = "Danh sách Cấu hình Affiliate",
                functions = "Cấu hình",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("detail/{id}")]
        public JsonResult GetDetail(Guid id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
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
                api_name = "api/affiliateconfig/detail/" + id,
                actions = "Chi tiết Cấu hình Affiliate",
                application = "WEB ADMIN",
                content = "Chi tiết Cấu hình Affiliate",
                functions = "Cấu hình",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("getGeneral")]
        public JsonResult GetGeneral(Guid id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getDetailGeneral();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/affiliateconfig/getGeneral",
                actions = "Chi tiết Cấu hình Affiliate",
                application = "WEB ADMIN",
                content = "Chi tiết Cấu hình Affiliate",
                functions = "Cấu hình",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("create")]
        [HttpPost]
        public JsonResult Create(AffiliateConfigRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Add))
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
                api_name = "api/affiliateconfig/create",
                actions = "Tạo mới Cấu hình Affiliate",
                application = "WEB ADMIN",
                content = "Tạo mới Cấu hình Affiliate",
                functions = "Cấu hình",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("update")]
        [HttpPost]
        public JsonResult Update(AffiliateConfigRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Edit))
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
                api_name = "api/affiliateconfig/update",
                actions = "Cập nhật Cấu hình Affiliate",
                application = "WEB ADMIN",
                content = "Cập nhật Cấu hình Affiliate",
                functions = "Cấu hình",
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Delete))
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
                    api_name = "api/affiliateconfig/delete",
                    actions = "Xóa Cấu hình Affiliate",
                    application = "WEB ADMIN",
                    content = "Xóa Cấu hình Affiliate",
                    functions = "Cấu hình",
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