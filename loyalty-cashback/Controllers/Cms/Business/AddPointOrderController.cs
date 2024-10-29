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
    [Route("api/addpointorder")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class AddPointOrderController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAdminAddPointOrder _app;
        private readonly ILoggingHelpers _logging;
        public AddPointOrderController(IConfiguration configuration, IAdminAddPointOrder app, ILoggingHelpers logging)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(AddPointOrderRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            APIResponse data = _app.getList(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/addpointorder/list",
                actions = "Danh sách Phiếu nạp điểm",
                application = "WEB ADMIN",
                content = "Danh sách Phiếu nạp điểm",
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
