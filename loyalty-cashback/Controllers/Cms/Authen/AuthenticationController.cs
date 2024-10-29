using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace LOYALTY.Controllers
{

    public class user_permissions
    {
        public Guid? id { get; set; }
        public string function_code { get; set; }
        public string function_name { get; set; }
        public string path { get; set; }
        public List<action> actions { get; set; }
    }

    public class action
    {
        public Guid? action_id { get; set; }
        public string action_code { get; set; }
        public string action_name { get; set; }
        public int? action_type { get; set; }
        public string path { get; set; }
    }
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IJwtAuth jwtAuth;
        private readonly ILoggingHelpers _loggingHelpers;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ICommonFunction _commonFunction;
        public AuthenticationController(IDistributedCache distributedCache, IJwtAuth jwtAuth, ILoggingHelpers iLoggingHelpers, LOYALTYContext context, IEmailSender emailSender, ICommonFunction commonFunction)
        {
            _distributedCache = distributedCache;
            this.jwtAuth = jwtAuth;
            _loggingHelpers = iLoggingHelpers;
            _context = context;
            _emailSender = emailSender;
            _commonFunction = commonFunction;
        }

        // API Đăng nhập Web Admin
        [AllowAnonymous]
        [Route("adminLogin")]
        [HttpPost]
        public JsonResult AdminLogin(LoginRequest loginRequest)
        {
            try
            {
                // Check Username
                var checkUserName = _context.Users.Where(x => x.username == loginRequest.username && x.is_admin == true && x.status == 1).FirstOrDefault();
                if (checkUserName == null)
                {
                    return new JsonResult(new APIResponse("ERROR_USERNAME_NOT_EXISTS")) { StatusCode = 200 };
                }

                if (checkUserName.password != _commonFunction.ComputeSha256Hash(loginRequest.password))
                {
                    return new JsonResult(new APIResponse("ERROR_PASSWORD_INCORRECT")) { StatusCode = 200 };
                }

                var listFunctionIds = (from p in _context.Actions
                                       join up in _context.UserPermissions on p.id equals up.action_id
                                       join f in _context.Functions on p.function_id equals f.id
                                       where up.user_id == checkUserName.id
                                       select f.id).ToList();

                var user_permissions = (from p in _context.Functions
                                        where p.status == 1 && listFunctionIds.Contains(p.id)
                                        select new user_permissions
                                        {
                                            id = p.id,
                                            function_code = p.code,
                                            function_name = p.name,
                                            path = p.url,
                                            actions = (from i in _context.Actions
                                                       join up in _context.UserPermissions on i.id equals up.action_id
                                                       where i.function_id == p.id && up.user_id == checkUserName.id
                                                       select new action
                                                       {
                                                           action_id = i.id,
                                                           action_code = i.code,
                                                           action_name = i.name,
                                                           path = i.url,
                                                           action_type = i.action_type
                                                       }).ToList()
                                        }).ToList();

                string allpermissions = CheckRole.convertListFuncTotring(user_permissions);


                var token = jwtAuth.Authentication(loginRequest.username, (Guid)checkUserName.id, _commonFunction.ComputeSha256Hash(loginRequest.password), Consts.USER_TYPE_WEB_ADMIN, allpermissions);
                if (token == null)
                {
                    return new JsonResult(new APIResponse("ERROR_SERVER")) { StatusCode = 200 };
                }

                // Login Response
                object loginResponse = new
                {
                    token = token,
                    user_id = checkUserName.id,
                    username = checkUserName.username,
                    full_name = checkUserName.full_name,
                    avatar = checkUserName.avatar,
                    is_admin = checkUserName.is_admin,
                    is_sysadmin = checkUserName.is_sysadmin,
                    user_permissions = user_permissions,
                    group_id = checkUserName.user_group_id,
                    allpermissions = allpermissions
                };

                // Ghi log
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _loggingHelpers.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    actions = "Đăng nhập",
                    api_name = "/api/auth/adminLogin",
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
                return new JsonResult(new APIResponse(ex)) { StatusCode = 200 };
            }
        }

        // API Đăng xuất Web Admin
        [Route("adminLogout")]
        [Authorize(Policy = "WebAdminUser")]
        [HttpPost]
        public JsonResult AdminLogout()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/auth/adminLogout",
                actions = "Đăng xuất",
                application = "WEB ADMIN",
                content = "Đăng xuất",
                functions = "Hệ thống",
                is_login = true,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });

            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Đổi mật khẩu Web Admin
        [Route("adminChangePass")]
        [Authorize(Policy = "WebAdminUser")]
        [HttpPost]
        public JsonResult AdminChangePassword(PasswordRequest pwdRequest)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var user = _context.Users.Where(x => x.username == username.Value && x.is_admin == true).FirstOrDefault();

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

            if (!_commonFunction.ValidatePassword(pwdRequest.new_password))
            {
                return new JsonResult(new APIResponse("ERROR_PASSWORD_NOT_VALID")) { StatusCode = 200 };
            }

            if (user.password != _commonFunction.ComputeSha256Hash(pwdRequest.old_password))
            {
                return new JsonResult(new APIResponse("ERROR_OLD_PASSWORD_NOT_INCORRECT")) { StatusCode = 200 };
            }

            try
            {
                var newNoti1 = new Notification();
                newNoti1.id = Guid.NewGuid();
                newNoti1.title = "Khóa tài khoản";
                newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                newNoti1.user_id = user.id;
                newNoti1.date_created = DateTime.Now;
                newNoti1.date_updated = DateTime.Now;
                newNoti1.content = "Tài khoản" + user.full_name + "đã bị khóa vui lòng đăng nhập lại";
                newNoti1.system_type = "Locked";

                _context.Notifications.Add(newNoti1);

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
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/auth/adminChangePass",
                actions = "Đổi mật khẩu",
                application = "WEB ADMIN",
                content = "Đổi mật khẩu",
                functions = "Hệ thống",
                is_login = true,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });

            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Gửi mã xác nhận quên mật khẩu
        [AllowAnonymous]
        [Route("sendCodeForgetAdmin")]
        [HttpPost]
        public JsonResult SendCodeForgetPassword(PasswordRequest pwdRequest)
        {
            if (pwdRequest.phone_number == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_MISSING")) { StatusCode = 200 };
            }

            var user = _context.Users.Where(x => x.phone == pwdRequest.phone_number && x.is_admin == true).FirstOrDefault();
            if (user == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NOT_EXISTS")) { StatusCode = 200 };
            }
            try
            {
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.phone_number = pwdRequest.phone_number;
                otp.otp_code = code.ToString();
                otp.date_created = DateTime.Now;
                otp.object_type = "ADMIN_FORGET_PASSWORD";
                otp.date_limit = DateTime.Now.AddMinutes(15);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(pwdRequest.phone_number, "CashPlus: Ma OTP xac nhan dat lai ma bao mat tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code);
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/auth/sendCodeForgetAdmin",
                actions = "Gửi mã OTP Quên mật khẩu",
                application = "WEB ADMIN",
                content = "Gửi mã OTP Quên mật khẩu",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API kiểm tra mã xác nhận
        [AllowAnonymous]
        [Route("confirmCodeForgetAdmin")]
        [HttpPost]
        public JsonResult ConfirmCodeForgetPassword(PasswordRequest pwdRequest)
        {
            if (pwdRequest.otp_code == null)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == pwdRequest.otp_code && x.date_limit > DateTime.Now && x.object_type == "ADMIN_FORGET_PASSWORD" && x.phone_number == pwdRequest.phone_number).FirstOrDefault();

            if (otp == null)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/auth/confirmCodeForgetAdmin",
                actions = "Kiểm tra mã OTP Quên mật khẩu",
                application = "WEB ADMIN",
                content = "Kiểm tra mã OTP Quên mật khẩu",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API lấy lại mật khẩu
        [AllowAnonymous]
        [Route("changePassForgetAdmin")]
        [HttpPost]
        public JsonResult ChangePassForgetPassword(PasswordRequest pwdRequest)
        {
            if (pwdRequest.otp_code == null)    
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == pwdRequest.otp_code && x.date_limit > DateTime.Now && x.object_type == "ADMIN_FORGET_PASSWORD" && x.phone_number == pwdRequest.phone_number).FirstOrDefault();

            if (otp == null)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            if (pwdRequest.phone_number == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_MISSING")) { StatusCode = 200 };
            }

            if (pwdRequest.new_password == null)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_PASSWORD_MISSING")) { StatusCode = 200 };
            }

            var user = _context.Users.Where(x => x.phone == pwdRequest.phone_number && x.is_admin == true).FirstOrDefault();
            if (user == null)
            {
                return new JsonResult(new APIResponse("ERROR_USER_NOT_EXISTS")) { StatusCode = 200 };
            }

            try
            {
                user.password = _commonFunction.ComputeSha256Hash(pwdRequest.new_password);
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
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/auth/changePassForgetAdmin",
                actions = "Tạo mật khẩu mới Quên mật khẩu",
                application = "WEB ADMIN",
                content = "Tạo mật khẩu mới Quên mật khẩu",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Lấy thông tin cá nhân
        [Route("adminGetUserInfo")]
        [Authorize(Policy = "WebAdminUser")]
        [HttpGet]
        public JsonResult AdminGetUserInfo()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            var user = _context.Users.Where(x => x.username == username.Value && x.is_admin == true).FirstOrDefault();

            if (user == null)
            {
                return new JsonResult(new APIResponse("ERROR_USER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userGroup = new UserGroup();
            if (user.user_group_id != null)
            {
                userGroup = _context.UserGroups.Where(x => x.id == user.user_group_id).FirstOrDefault();
            }

            var listFunctionIds = (from p in _context.Actions
                                   join up in _context.UserPermissions on p.id equals up.action_id
                                   join f in _context.Functions on p.function_id equals f.id
                                   where up.user_id == user.id
                                   select f.id).ToList();


            var user_permissions = (from p in _context.Functions
                                    where p.status == 1 && listFunctionIds.Contains(p.id)
                                    select new
                                    {
                                        id = p.id,
                                        function_code = p.code,
                                        function_name = p.name,
                                        path = p.url,
                                        actions = (from i in _context.Actions
                                                   join up in _context.UserPermissions on i.id equals up.action_id
                                                   where i.function_id == p.id && up.user_id == user.id
                                                   select new
                                                   {
                                                       action_id = i.id,
                                                       action_code = i.code,
                                                       action_name = i.name,
                                                       path = i.url
                                                   }).ToList()
                                    }).ToList();

            var userInfoResponse = new
            {
                id = user.id,
                avatar = user.avatar,
                email = user.email,
                full_name = user.full_name,
                phone = user.phone,
                username = user.username,
                user_group_id = user.user_group_id,
                user_group_name = userGroup == null ? "" : userGroup.name,
                is_sysadmin = user.is_sysadmin,
                is_admin = user.is_admin,
                user_permissions = user_permissions
            };

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/auth/adminGetUserInfo",
                actions = "Lấy thông tin cá nhân",
                application = "WEB ADMIN",
                content = "Lấy thông tin cá nhân user: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(userInfoResponse)) { StatusCode = 200 };
        }

        // API Cập nhật thông tin cá nhân
        [Route("adminUpdateUserInfo")]
        [Authorize(Policy = "WebAdminUser")]
        [HttpPost]
        public JsonResult AdminUpdateUserInfo(User userRequest)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            var user = _context.Users.Where(x => x.username == username.Value && x.is_admin == true).FirstOrDefault();

            if (user == null)
            {
                return new JsonResult(new APIResponse("ERROR_USER_NOT_EXISTS")) { StatusCode = 200 };
            }

            try
            {
                user.full_name = userRequest.full_name;
                user.email = userRequest.email;
                user.phone = userRequest.phone;
                user.avatar = userRequest.avatar;
                user.address = userRequest.address;
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
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/auth/adminUpdateUserInfo",
                actions = "Cập nhật thông tin cá nhân",
                application = "WEB ADMIN",
                content = "Cập nhật thông tin cá nhân user: " + username.Value,
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
