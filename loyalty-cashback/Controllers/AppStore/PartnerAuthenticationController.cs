using LOYALTY.CloudMessaging;
using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using LOYALTY.PaymentGate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LOYALTY.Controllers
{
    [Route("api/app/pauth")]
    [ApiController]
    public class PartnerAuthenticationController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IJwtAuth jwtAuth;
        private readonly ILoggingHelpers _loggingHelpers;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ICommonFunction _commonFunction;
        private readonly ICommon _common;
        private readonly FCMNotification _fcmNotification;
        private readonly BKTransaction _bkTransaction;
        private readonly IConfiguration _configuration;

        public PartnerAuthenticationController(IDistributedCache distributedCache, IJwtAuth jwtAuth, ILoggingHelpers iLoggingHelpers, LOYALTYContext context, IEmailSender emailSender, ICommonFunction commonFunction, ICommon common, FCMNotification fCMNotification, BKTransaction bkTransaction, IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            this.jwtAuth = jwtAuth;
            _loggingHelpers = iLoggingHelpers;
            _context = context;
            _emailSender = emailSender;
            _commonFunction = commonFunction;
            _common = common;
            _fcmNotification = fCMNotification;
            _bkTransaction = bkTransaction;
            _configuration = configuration;
        }

        // API Đăng nhập tk 
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<JsonResult> Login(LoginRequest loginRequest)
        {
            try
            {
                // Check Username
                var checkUserName = _context.Users.Where(x => x.username == loginRequest.username && x.is_partner == true && (x.is_delete == null || (x.is_delete != null && x.is_delete != true))).FirstOrDefault();
                if (checkUserName == null)
                {
                    return new JsonResult(new APIResponse("ERROR_USERNAME_NOT_EXISTS")) { StatusCode = 200 };
                }

                if (checkUserName.status != 1)
                {
                    return new JsonResult(new APIResponse("ERROR_USER_IS_LOCK")) { StatusCode = 200 };
                }

                var partnerObj = _context.Partners.Where(x => x.id == checkUserName.partner_id).FirstOrDefault();

                if (partnerObj == null)
                {
                    return new JsonResult(new APIResponse("ERROR_USERNAME_NOT_EXISTS")) { StatusCode = 200 };
                }

                if (partnerObj.status != 15)
                {
                    return new JsonResult(new APIResponse("ERROR_USER_IS_LOCK")) { StatusCode = 200 };
                }

                if (checkUserName.password != _commonFunction.ComputeSha256Hash(loginRequest.password))
                {
                    return new JsonResult(new APIResponse("ERROR_USER_WRONG_PASSWORD")) { StatusCode = 200 };
                }

                var contractObj = _context.PartnerContracts.Where(x => x.partner_id == partnerObj.id && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12).Select(x => x.id).FirstOrDefault();

                if (contractObj == null)
                {
                    return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_HAVE_CONTRACT")) { StatusCode = 200 };
                }

                var token = jwtAuth.BranchAuthentication(loginRequest.username, _commonFunction.ComputeSha256Hash(loginRequest.password), Consts.USER_TYPE_WEB_PARTNER, (Guid)checkUserName.partner_id);
                if (token == null)
                {
                    return new JsonResult(new APIResponse("ERROR_SERVER")) { StatusCode = 200 };
                }

                var customerFakeAccount = _context.CustomerFakeBanks.Where(x => x.user_id == checkUserName.partner_id && x.supplier == "BAOKIM").FirstOrDefault();

                if (customerFakeAccount == null)
                {
                    string privatekey = _context.Partners.Where(l => l.id == (Guid)checkUserName.partner_id).Select(p => p.RSA_privateKey).FirstOrDefault();
                    if (privatekey != "")
                    {
                        string createFakeAccount = await _bkTransaction.createVirtualAccount((Guid)checkUserName.partner_id, privatekey);
                    }
                }

                if (loginRequest.device_id != null)
                {
                    if (checkUserName.device_id != null && checkUserName.device_id.Length > 0 && loginRequest.device_id != checkUserName.device_id)
                    {
                        // Bắn Notification Firebase
                        string message = await _fcmNotification.SendNotification(checkUserName.device_id, "LOGIN_OTHER_DEVICE", "Cảnh báo", "Tài khoản của bạn đã được đăng nhập trên thiết bị khác. Nếu không phải là bạn vui lòng kiểm tra lại tài khoản.", null);
                    }
                    // Cập nhật device_id
                    checkUserName.device_id = loginRequest.device_id;

                    _context.SaveChanges();
                }

                var settingObj = _context.Settingses.FirstOrDefault();
                Boolean is_review = false;
                if (settingObj != null && settingObj.is_review != null && settingObj.is_review == true)
                {
                    is_review = true;
                }
                // Login Response
                object loginResponse = new
                {
                    token = token,
                    user_id = checkUserName.id,
                    username = checkUserName.username,
                    full_name = checkUserName.full_name,
                    avatar = checkUserName.avatar,
                    email = checkUserName.email,
                    phone = checkUserName.phone,
                    is_partner = checkUserName.is_partner,
                    is_partner_admin = checkUserName.is_partner_admin,
                    partner_id = checkUserName.partner_id,
                    is_manage_user = checkUserName.is_partner_admin == true ? true : (checkUserName.is_manage_user != null ? checkUserName.is_manage_user : false),
                    is_add_point_permission = checkUserName.is_partner_admin == true ? true : (checkUserName.is_add_point_permission != null ? checkUserName.is_add_point_permission : false),
                    is_change_point_permission = checkUserName.is_partner_admin == true ? true : (checkUserName.is_change_point_permission != null ? checkUserName.is_change_point_permission : false),
                    is_have_secure_code = checkUserName.secret_key != null ? true : false,
                    is_review = is_review,
                    send_Notification = checkUserName.send_Notification,
                    send_Popup = checkUserName.send_Popup,
                    SMS_addPointSave = checkUserName.SMS_addPointSave,
                    SMS_addPointUse = checkUserName.SMS_addPointUse
                };

                // Ghi log
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                await _loggingHelpers.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    actions = "Đăng nhập",
                    api_name = "/api/pauth/adminLogin",
                    application = "WEB ADMIN",
                    content = loginResponse.ToString(),
                    functions = "Hệ thống",
                    is_login = true,
                    result_logging = "Thành công",
                    user_created = checkUserName.username,
                    IP = remoteIP.ToString()
                });
                return new JsonResult(new APIResponse(loginResponse)) { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(ex)) { StatusCode = 500 };
            }
        }

        // API Khóa TK
        [Route("lockUser")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpGet]
        public JsonResult LockUser()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            Guid partner_id = Guid.Parse(partnerToken.Value);
            var partnerObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_MISSING")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.partner_id == partnerObj.id && x.username == username.Value && x.is_partner == true).FirstOrDefault();

            if (userObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_MISSING")) { StatusCode = 200 };
            }

            userObj.status = 2;
            partnerObj.status = 16;

            _context.SaveChanges();


            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/lockUser",
                actions = "Khóa tài khoản",
                application = "APP LOYALTY",
                content = "Khóa tài khoản",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Gửi mã xác nhận tạo mã bảo mật
        [Route("sendCodeCreateSecretKey")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpGet]
        public JsonResult AppSendCodeCreateSecretKey()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }
            
            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            try
            {
                if(partnerObj.time_otp_limit != null && partnerObj.time_otp_limit > DateTime.Now){
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);
                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.phone_number = partnerObj.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "PARTNER_CREATE_SECRET_KEY";
                otp.date_limit = DateTime.Now.AddMinutes(2);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(partnerObj.phone, "CashPlus: Ma OTP xac nhan tao ma bao mat tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/sendCodeCreateSecretKey",
                actions = "Gửi mail mã xác nhận tạo mật khẩu giao dịch",
                application = "APP PARTNER",
                content = "Gửi mail mã xác nhận tạo mật khẩu giao dịch tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Tạo mã bảo mật
        [Route("createSecretKey")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult AppCreateSecretKey(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            if (userObj.secret_key != null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_HAVE_SECRET_KEY")) { StatusCode = 200 };
            }

            if (req.new_secret_key == null || req.new_secret_key.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MISSING")) { StatusCode = 200 };
            }

            if (req.new_secret_key.Length != 6)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MUST_BE_LENGTH_EQUALS_6")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "PARTNER_CREATE_SECRET_KEY" && x.phone_number == partnerObj.phone).FirstOrDefault();

            if (otp == null)
            {
               partnerObj.count_otp_fail = partnerObj.count_otp_fail != null ? partnerObj.count_otp_fail + 1 : 1;
                if(partnerObj.count_otp_fail >= 3){
                    partnerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    partnerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            try
            {
                userObj.secret_key = _commonFunction.ComputeSha256Hash(req.new_secret_key);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/createSecretKey",
                actions = "Tạo mã bảo mật",
                application = "APP PARTNER",
                content = "Tạo mã bảo mật tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Gửi mã xác nhận đổi mã bảo mật
        [Route("sendCodeChangeSecretKey")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpGet]
        public JsonResult AppSendCodeChangeSecretKey()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            try
            {
                if (partnerObj.time_otp_limit != null && partnerObj.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.phone_number = partnerObj.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "PARTNER_CHANGE_SECRET_KEY";
                otp.date_limit = DateTime.Now.AddMinutes(2);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(partnerObj.phone, "CashPlus: Ma OTP xac nhan thay doi ma bao mat tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/sendCodeForgetSecretKey",
                actions = "Gửi mail mã xác nhận quên mật khẩu giao dịch",
                application = "APP PARTNER",
                content = "Gửi mail mã xác nhận quên mật khẩu giao dịch tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Đổi mã bảo mật
        [Route("changeSecretKey")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult AppChangeSecretKey(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            if (req.old_secret_key == null || req.old_secret_key.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OLD_SECRET_KEY_MISSING")) { StatusCode = 200 };
            }

            if (req.new_secret_key == null || req.new_secret_key.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MISSING")) { StatusCode = 200 };
            }

            if (req.new_secret_key.Length != 6)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MUST_BE_LENGTH_EQUALS_6")) { StatusCode = 200 };
            }

            if (userObj.secret_key != _commonFunction.ComputeSha256Hash(req.old_secret_key))
            {
                return new JsonResult(new APIResponse("ERROR_OLD_SECRET_KEY_INCORRECT")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "PARTNER_CHANGE_SECRET_KEY" && x.phone_number == partnerObj.phone).FirstOrDefault();

            if (otp == null)
            {
               partnerObj.count_otp_fail = partnerObj.count_otp_fail != null ? partnerObj.count_otp_fail + 1 : 1;
                if (partnerObj.count_otp_fail >= 3)
                {
                    partnerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    partnerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            try
            {
                userObj.secret_key = _commonFunction.ComputeSha256Hash(req.new_secret_key);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/changeSecretKey",
                actions = "Đổi mã bảo mật",
                application = "APP PARTNER",
                content = "Đổi mã bảo mật tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Gửi mã xác nhận quên mã bảo mật
        [Route("sendCodeForgetSecretKey")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpGet]
        public JsonResult AppSendCodeForgetSecretKey()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            try
            {
                if (partnerObj.time_otp_limit != null &&  partnerObj.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.phone_number = partnerObj.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "PARTNER_FORGET_SECRET_KEY";
                otp.date_limit = DateTime.Now.AddMinutes(2);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(partnerObj.phone, "CashPlus: Ma OTP xac nhan dat lai ma bao mat tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/sendCodeForgetSecretKey",
                actions = "Gửi mail mã xác nhận quên mật khẩu giao dịch",
                application = "APP PARTNER",
                content = "Gửi mail mã xác nhận quên mật khẩu giao dịch tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Tạo mã bảo mật từ lúc quên
        [Route("renewSecretKey")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult AppRenewSecretKey(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            if (req.otp_code == null || req.otp_code.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            if (req.new_secret_key == null || req.new_secret_key.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MISSING")) { StatusCode = 200 };
            }

            if (req.new_secret_key.Length != 6)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MUST_BE_LENGTH_EQUALS_6")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "PARTNER_FORGET_SECRET_KEY" && x.phone_number == partnerObj.phone).FirstOrDefault();

            if (otp == null)
            {
               partnerObj.count_otp_fail = partnerObj.count_otp_fail != null ? partnerObj.count_otp_fail + 1 : 1;
                if (partnerObj.count_otp_fail >= 3)
                {
                    partnerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    partnerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            try
            {
                userObj.secret_key = _commonFunction.ComputeSha256Hash(req.new_secret_key);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/renewSecretKey",
                actions = "Tạo mật khẩu giao dịch mới khi quên mật khẩu giao dịch",
                application = "APP PARTNER",
                content = "Tạo mật khẩu giao dịch mới khi quên mật khẩu giao dịch tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Cập nhật thông tin cá nhân
        [Route("updateCusInfo")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult AppUpdatePartnerInfo(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Users.Where(x => x.is_partner == true && x.username == username.Value && x.partner_id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (req.full_name == null)
            {
                return new JsonResult(new APIResponse("ERROR_FULL_NAME_MISSING")) { StatusCode = 200 };
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                partnerObj.send_Notification = req.send_Notification != null ? req.send_Notification : false;
                partnerObj.send_Popup = req.send_Popup != null ? req.send_Popup : false;
                partnerObj.SMS_addPointSave = req.SMS_addPointSave != null ? req.SMS_addPointSave : false;
                partnerObj.SMS_addPointUse = req.SMS_addPointUse != null ? req.SMS_addPointUse : false;
                partnerObj.full_name = req.full_name;
                partnerObj.email = req.email;
                partnerObj.phone = req.phone;
                partnerObj.address = req.address;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            transaction.Commit();
            transaction.Dispose();
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/updateCusInfo",
                actions = "Cập nhật thông tin tài khoản đối tác",
                application = "APP PARTNER",
                content = "Cập nhật thông tin tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Cập nhật ảnh dại diện
        [Route("updateCusAvatar")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult AppUpdatePartnerAvatar(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Users.Where(x => x.is_partner == true && x.username == username.Value && x.partner_id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (req.avatar == null || req.avatar.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_LOGO_MISSING")) { StatusCode = 200 };
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                partnerObj.avatar = req.avatar;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            transaction.Commit();
            transaction.Dispose();
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/updateCusAvatar",
                actions = "Cập nhật ảnh đại diện tài khoản đối tác",
                application = "APP PARTNER",
                content = "Cập nhật ảnh đại diện tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Đổi mật khẩu Đối tác
        [Route("changePass")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult AppChangePassword(PasswordRequest pwdRequest)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            var user = _context.Users.Where(x => x.username == username.Value && x.is_partner == true && x.partner_id == partnerObj.id).FirstOrDefault();

            if (pwdRequest.old_password == null)
            {
                return new JsonResult(new APIResponse("ERROR_OLD_PASSWORD_MISSING")) { StatusCode = 200 };
            }

            if (pwdRequest.new_password == null)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_PASSWORD_MISSING")) { StatusCode = 200 };
            }

            if (user == null)
            {
                return new JsonResult(new APIResponse("ERROR_USER_NOT_EXISTS")) { StatusCode = 200 };
            }

            if (user.password != _commonFunction.ComputeSha256Hash(pwdRequest.old_password))
            {
                return new JsonResult(new APIResponse("ERROR_OLD_PASSWORD_NOT_INCORRECT")) { StatusCode = 200 };
            }

            if (!_commonFunction.ValidatePassword(pwdRequest.new_password))
            {
                return new JsonResult(new APIResponse("ERROR_PASSWORD_INCORRECT")) { StatusCode = 200 };
            }

            try
            {
                user.password = _commonFunction.ComputeSha256Hash(pwdRequest.new_password);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse("ERROR_CHANGE_PASS_FAIL")) { StatusCode = 200 };
            }
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/changePass",
                actions = "Đổi mật khẩu",
                application = "APP PARTNER",
                content = "Đổi mật khẩu",
                functions = "Hệ thống",
                is_login = true,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });

            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Đăng xuất
        [Route("logout")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult BranchLogout()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            var userObj = _context.Users.Where(x => x.is_partner == true && x.username == username.Value).FirstOrDefault();

            try
            {
                if (userObj != null)
                {
                    userObj.device_id = null;
                    _context.SaveChanges();
                }
            }
            catch
            {

            }

            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/logout",
                actions = "Đăng xuất",
                application = "APP PARTNER",
                content = "Đăng xuất",
                functions = "Hệ thống",
                is_login = true,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });

            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Gửi tiền
        [Route("sendFirmBank")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult sendFirmBank()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            TransferResponseObj response = _bkTransaction.transferMoney(Consts.CP_BK_PARTNER_CODE, "970422", "3330122031993", "PHAM TUAN ANH", 3000, "Chuyen tien", Consts.private_key);
            var result = JsonConvert.SerializeObject(response);
            return new JsonResult(new APIResponse(new
            {
                result = result
            }))
            { StatusCode = 200 };
        }

        // API Gửi mã xác nhận quên mật khẩu
        [AllowAnonymous]
        [Route("sendCodeForgetPassword")]
        [HttpPost]
        public async Task<JsonResult> AppSendCodeForgetPassword(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            if (req.username == null || req.username.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_USERNAME_MISSING")) { StatusCode = 200 };
            }

            var checkUser = _context.Users.Where(x => x.username == req.username && x.is_partner_admin == true).FirstOrDefault();
            // var checkUser = _context.Users.Where(x => x.phone == req.username && x.is_partner_admin == true).FirstOrDefault();

            if (checkUser == null)
            {
                return new JsonResult(new APIResponse("ERROR_USERNAME_NOT_EXISTS")) { StatusCode = 200 };
            }

            var partnerObj = _context.Partners.Where(x => x.id == checkUser.partner_id).FirstOrDefault();
            if(partnerObj == null){
                return new JsonResult(new APIResponse("ERROR_USERNAME_MISSING")) { StatusCode = 200 };
            }

            if (checkUser.status != 1 || partnerObj.status != 15)
            {
                return new JsonResult(new APIResponse("ERROR_USER_LOCK")) { StatusCode = 200 };
            }

            try
            {
                if (partnerObj != null)
                {
                    if (partnerObj.time_otp_limit != null &&  partnerObj.time_otp_limit > DateTime.Now)
                    {
                        return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                    }
                    Random rnd = new Random();
                    int code = rnd.Next(100000, 999999);

                    var otp = new OTPTransaction();
                    otp.phone_number = partnerObj.phone;
                    otp.otp_code = code.ToString();
                    otp.object_name = partnerObj.phone;
                    otp.date_created = DateTime.Now;
                    otp.object_type = "PARTNER_FORGET_PASSWORD";
                    otp.date_limit = DateTime.Now.AddMinutes(2);
                    _context.OTPTransactions.Add(otp);
                    _context.SaveChanges();

                    if (partnerObj.phone != null)
                    {
                        await _emailSender.SendSms(partnerObj.phone, "CashPlus: Ma OTP xac nhan dat lai mat khau tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/pauth/sendCodeForgetPassword",
                actions = "Gửi mail mã xác nhận quên mật khẩu cửa hàng",
                application = "APP PARTNER",
                content = "Gửi mail mã xác nhận quên mật khẩu cửa hàng: " + req.phone,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = req.phone,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Confirm mã OTP quên mật khẩu
        [Route("confirmForgetPassword")]
        [HttpPost]
        public JsonResult AppConfirmCustomerForgetPassword(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            if (req.username == null || req.username.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_USERNAME_MISSING")) { StatusCode = 200 };
            }

            var checkUser = _context.Users.Where(x => x.username == req.username && x.is_partner_admin == true).FirstOrDefault();

            if (checkUser == null)
            {
                return new JsonResult(new APIResponse("ERROR_USERNAME_NOT_EXISTS")) { StatusCode = 200 };
            }

            var partnerObj = _context.Partners.Where(x => x.id == checkUser.partner_id).FirstOrDefault();

            if (req.otp_code == null || req.otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "PARTNER_FORGET_PASSWORD" && x.phone_number == partnerObj.phone).FirstOrDefault();

            if (otp == null)
            {
               partnerObj.count_otp_fail = partnerObj.count_otp_fail != null ? partnerObj.count_otp_fail + 1 : 1;
                if (partnerObj.count_otp_fail >= 3)
                {
                    partnerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    partnerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Cập nhật mật khẩu khi quên
        [Route("updateForgetPassword")]
        [HttpPost]
        public JsonResult AppUpdateCustomerForgetPassword(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            if (req.username == null || req.username.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_USERNAME_MISSING")) { StatusCode = 200 };
            }

            var checkUser = _context.Users.Where(x => x.username == req.username && x.is_partner_admin == true).FirstOrDefault();

            if (checkUser == null)
            {
                return new JsonResult(new APIResponse("ERROR_USERNAME_NOT_EXISTS")) { StatusCode = 200 };
            }

            var partnerObj = _context.Partners.Where(x => x.id == checkUser.partner_id).FirstOrDefault();

            if (req.password == null || req.password.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PASSWORD_MISSING")) { StatusCode = 200 };
            }

            //// Check password

            if (!_commonFunction.ValidatePassword(req.password))
            {
                return new JsonResult(new APIResponse(Messages.ERROR_PASSWORD_INCORRECT_PATTERN));
            }

            if (req.otp_code == null || req.otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "PARTNER_FORGET_PASSWORD" && x.phone_number == partnerObj.phone).FirstOrDefault();

            if (otp == null)
            {
               partnerObj.count_otp_fail = partnerObj.count_otp_fail != null ? partnerObj.count_otp_fail + 1 : 1;
                if (partnerObj.count_otp_fail >= 3)
                {
                   partnerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    partnerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                if (!_commonFunction.ValidatePassword(req.password))
                {
                    return new JsonResult(new APIResponse("ERROR_PASSWORD_INCORRECT")) { StatusCode = 200 };
                }
                checkUser.password = _commonFunction.ComputeSha256Hash(req.password);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            transaction.Commit();
            transaction.Dispose();
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/pauth/updateForgetPassword",
                actions = "Cập nhật mật khẩu khi quên",
                application = "APP PARTNER",
                content = "Cập nhật mật khẩu khi quên: " + req.username,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = req.phone,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Check mã cũ
        [Route("checkOldSecretKeyMerchant")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult checkOldSecretKeyMerchant(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            if (req.old_secret_key == null || req.old_secret_key.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OLD_SECRET_KEY_MISSING")) { StatusCode = 200 };
            }


            if (userObj.secret_key != _commonFunction.ComputeSha256Hash(req.old_secret_key))
            {
                return new JsonResult(new APIResponse("ERROR_OLD_SECRET_KEY_INCORRECT")) { StatusCode = 200 };
            }
            try
            {
                return new JsonResult(new APIResponse(true)) { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
        }

        // API Đổi mã bảo mật ver2
        [Route("changeSecretKeyMerchant")]
        [Authorize(Policy = "WebPartnerUser")]
        [HttpPost]
        public JsonResult changeSecretKeyMerchant(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var partnerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == Guid.Parse(partnerToken.Value)).FirstOrDefault();

            if (partnerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PARTNER_NOT_EXISTS")) { StatusCode = 200 };
            }
            var userObj = _context.Users.Where(x => x.is_partner == true && x.partner_id == partnerObj.id && x.username == username.Value).FirstOrDefault();

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "PARTNER_CHANGE_SECRET_KEY" && x.phone_number == partnerObj.phone).FirstOrDefault();

            if (otp == null)
            {
               partnerObj.count_otp_fail = partnerObj.count_otp_fail != null ? partnerObj.count_otp_fail + 1 : 1;
                if (partnerObj.count_otp_fail >= 3)
                {
                  partnerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    partnerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }


            if (req.new_secret_key == null || req.new_secret_key.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MISSING")) { StatusCode = 200 };
            }

            if (req.new_secret_key.Length != 6)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MUST_BE_LENGTH_EQUALS_6")) { StatusCode = 200 };
            }

            try
            {
                userObj.secret_key = _commonFunction.ComputeSha256Hash(req.new_secret_key);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_PARTNER,
                is_call_api = true,
                api_name = "api/app/pauth/changeSecretKey",
                actions = "Đổi mã bảo mật",
                application = "APP PARTNER",
                content = "Đổi mã bảo mật tài khoản đối tác: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }
    }
}
