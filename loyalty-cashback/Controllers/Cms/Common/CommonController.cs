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
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.CloudMessaging;

namespace LOYALTY.Controllers
{
    [Route("api/common")]
    [Authorize]
    [ApiController]
    public class CommonController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAppCustomerHome _Action;
        private readonly ILoggingHelpers _logging;
        private readonly IEncryptData _encryptData;
        private readonly LOYALTYContext _context;
        private readonly FCMNotification _fCMNotification;
        public CommonController(IConfiguration configuration, IAppCustomerHome Action, ILoggingHelpers logging, IEncryptData encryptData, LOYALTYContext context, FCMNotification fCMNotification)
        {
            _configuration = configuration;
            this._Action = Action;
            this._logging = logging;
            _encryptData = encryptData;
            _context = context;
            _fCMNotification = fCMNotification;
        }

        [Route("testFCM")]
        [HttpPost]
        public async Task<JsonResult> TestFCM(DecryptRequest req)
        {
            var dataResult = new
            {
                message = await _fCMNotification.SendNotification(req.secret_key, "TEST_FCM", "Test FCM Title", "Test FCM Body", null)
            };
            return new JsonResult(new APIResponse(dataResult)) { StatusCode = 200 };
        }

    }
}
