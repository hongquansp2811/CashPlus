using LOYALTY.CloudMessaging;
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
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LOYALTY.Controllers
{
    [Route("api/app/auth")]
    [ApiController]
    public class AppAuthenticationController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IJwtAuth jwtAuth;
        private readonly ILoggingHelpers _loggingHelpers;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ICommonFunction _commonFunction;
        private readonly ICommon _common;
        private readonly FCMNotification _fcmNotification;
        private readonly IConfiguration _configuration;

        public AppAuthenticationController(IDistributedCache distributedCache, IJwtAuth jwtAuth, ILoggingHelpers iLoggingHelpers, LOYALTYContext context, IEmailSender emailSender, ICommonFunction commonFunction, ICommon common, FCMNotification fCMNotification, IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            this.jwtAuth = jwtAuth;
            _loggingHelpers = iLoggingHelpers;
            _context = context;
            _emailSender = emailSender;
            _commonFunction = commonFunction;
            _common = common;
            _fcmNotification = fCMNotification;
            _configuration = configuration;
        }

        // API Gửi OTP Đăng ký
        [AllowAnonymous]
        [Route("sendOTPRegister")]
        [HttpPost]
        public JsonResult SendOTPRegister(LoginRequest loginRequest)
        {
            if (loginRequest.phone_number == null || loginRequest.phone_number.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_MISSING"));
            }

            if (loginRequest.phone_number.Contains("+84"))
            {
                loginRequest.phone_number = loginRequest.phone_number.Replace("+84", "0");
            }

            Boolean is_account = false;
            Boolean is_lock = false;

            var checkUser = (from p in _context.Customers
                             join u in _context.Users on p.id equals u.customer_id
                             where u.is_customer == true && p.phone == loginRequest.phone_number
                             select new
                             {
                                 customer_id = p.id,
                                 status = u.status,
                                 customer_status = p.status,
                                 is_delete = u.is_delete,
                                 otp_time_limit = p.time_otp_limit
                             }).FirstOrDefault();

            if (checkUser != null)
            {
                is_account = true;

                if (checkUser.status == 2 || checkUser.customer_status == 2)
                {
                    is_lock = true;
                }
                if(checkUser.otp_time_limit != null && checkUser.otp_time_limit > DateTime.Now){
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
            }
            else
            {
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.object_name = loginRequest.phone_number;
                otp.phone_number =  loginRequest.phone_number;
                otp.date_created = DateTime.Now;
                otp.object_type = "APP_REGISTER";
                otp.date_limit = DateTime.Now.AddMinutes(1);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(loginRequest.phone_number, "CashPlus: Ma OTP xac nhan dang ky tai khoan tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
            }

            var loginResponse = new
            {
                is_account = is_account,
                is_lock = true
            };
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "/api/app/auth/sendOTP",
                actions = "Gửi OTP Đăng ký tài khoản",
                application = "APP LOYALTY",
                content = "",
                functions = "Hệ thống",
                is_login = true,
                result_logging = "Thành công",
                user_created = "",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(loginResponse)) { StatusCode = 200 };
        }

        // API Khóa TK
        [Route("lockUser")]
        [Authorize(Policy = "AppUser")]
        [HttpGet]
        public JsonResult LockUser()
        {
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            Guid customer_id = Guid.Parse(customerToken.Value);
            var customerObj = _context.Customers.Where(x => x.id == customer_id).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_MISSING")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.customer_id == customerObj.id && x.is_customer == true).FirstOrDefault();

            if (userObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_MISSING")) { StatusCode = 200 };
            }

            userObj.status = 2;
            customerObj.status = 2;
            _context.SaveChanges();


            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/sendCodeVerify",
                actions = "Gửi mã OTP Xác minh tài khoản",
                application = "APP DCVF",
                content = "Gửi mã OTP Xác minh tài khoản",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Đăng ký tài khoản mới
        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        public async Task<JsonResult> Register(LoginRequest loginRequest)
        {
            if (loginRequest.phone_number == null || loginRequest.phone_number.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_MISSING"));
            }

            if (loginRequest.full_name == null || loginRequest.full_name.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_FULL_NAME_MISSING"));
            }

            if (loginRequest.phone_number.Contains("+84"))
            {
                loginRequest.phone_number = loginRequest.phone_number.Replace("+84", "0");
            }

            var checkUser = (from p in _context.Customers
                             join u in _context.Users on p.id equals u.customer_id
                             where u.is_customer == true && p.phone == loginRequest.phone_number
                             select new
                             {
                                 customer_id = p.id,
                                 status = u.status
                             }).FirstOrDefault();

            if (checkUser != null)
            {
                return new JsonResult(new APIResponse(Messages.ERROR_CUSTOMER_EXISTS));
            }

            if (loginRequest.otp_code == null || loginRequest.otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse(Messages.ERROR_OTP_CODE_MISSING));
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == loginRequest.otp_code && x.date_limit > DateTime.Now && x.object_type == "APP_REGISTER" && x.phone_number == loginRequest.phone_number).FirstOrDefault();

            if (otp == null)
            {
                return new JsonResult(new APIResponse(Messages.ERROR_OTP_CODE_INCORRECT)) { StatusCode = 200 };
            }

            if (loginRequest.password == null || loginRequest.password.Length == 0)
            {
                return new JsonResult(new APIResponse(Messages.ERROR_PASSWORD_MISSING));
            }

            var transaction = _context.Database.BeginTransaction();

            var customer = new Customer();
            string token = "";
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringReturn = new string(Enumerable.Repeat(chars, 9).Select(s => s[random.Next(s.Length)]).ToArray());
            try
            {
                // Tạo Khách hàng
                var newCustomer = new Customer();
                newCustomer.id = Guid.NewGuid();

                newCustomer.phone = loginRequest.phone_number;
                newCustomer.full_name = loginRequest.full_name;
                newCustomer.email = loginRequest.email;
                newCustomer.user_created = loginRequest.name;
                newCustomer.user_updated = loginRequest.name;
                newCustomer.date_created = DateTime.Now;
                newCustomer.date_updated = DateTime.Now;
                newCustomer.status = 1;

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();

                customer = newCustomer;
                var sharePerson = new User();

                if (loginRequest.share_code != null && loginRequest.share_code.Length > 0)
                {
                    sharePerson = _context.Users.Where(x => x.share_code == loginRequest.share_code).FirstOrDefault();
                }
                // Tạo tài khoản
                var newUser = new User();
                newUser.full_name = loginRequest.name;
                newUser.username = loginRequest.phone_number;
                newUser.password = _commonFunction.ComputeSha256Hash(loginRequest.password);
                newUser.status = 1;
                newUser.is_admin = false;
                newUser.is_sysadmin = false;
                newUser.is_customer = true;
                newUser.customer_id = newCustomer.id;
                newUser.is_delete = false;
                newUser.is_partner = false;
                newUser.is_partner_admin = false;
                newUser.total_point = 0;
                newUser.point_waiting = 0;
                newUser.point_avaiable = 0;
                newUser.point_affiliate = 0;
                newUser.device_id = loginRequest.device_id != null && loginRequest.device_id.Length > 0 ? loginRequest.device_id : null;
                if (sharePerson != null)
                {
                    newUser.share_person_id = sharePerson.id;
                }
                newUser.share_code = stringReturn;
                newUser.send_Notification = true;
                newUser.send_Popup = true;
                newUser.SMS_addPointSave = true;
                newUser.SMS_addPointUse = true;
                _context.Users.Add(newUser);

                await _context.SaveChangesAsync();
                // Tạo token trả ra
                token = jwtAuth.BranchAuthentication(loginRequest.phone_number, loginRequest.password, Consts.USER_TYPE_CUSTOMER, (Guid)newCustomer.id);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new APIResponse(Messages.ERROR_LOGIN_FAIL));
            }

            transaction.Commit();
            transaction.Dispose();

            var loginResponse = new
            {
                token = token,
                user_id = customer.id,
                username = customer.phone,
                full_name = customer.full_name,
                avatar = ""
            };
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "/api/app/auth/register",
                actions = "Đăng ký tài khoản",
                application = "APP LOYALTY",
                content = "",
                functions = "Hệ thống",
                is_login = true,
                result_logging = "Thành công",
                user_created = "",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(loginResponse)) { StatusCode = 200 };
        }

        // API Đăng nhập tk cũ
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<JsonResult> Login(LoginRequest loginRequest)
        {
            if (loginRequest.phone_number == null || loginRequest.phone_number.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_MISSING"));
            }
            if (loginRequest.phone_number.Contains("+84"))
            {
                loginRequest.phone_number = loginRequest.phone_number.Replace("+84", "0");
            }

            var checkUser = (from p in _context.Customers
                             join u in _context.Users on p.id equals u.customer_id
                             where u.is_customer == true && p.phone == loginRequest.phone_number
                             select new
                             {
                                 customer_id = p.id,
                                 full_name = p.full_name,
                                 phone = p.phone,
                                 avatar = p.avatar,
                                 password = u.password,
                                 status = u.status,
                                 customer_status = p.status,
                                 is_have_secure_code = u.secret_key == null ? false : true,
                                 device_id = u.device_id,
                                 send_Notification = u.send_Notification,
                                 send_Popup = u.send_Popup,
                                 SMS_addPointSave = u.SMS_addPointSave,
                                 SMS_addPointUse = u.SMS_addPointUse
                             }).FirstOrDefault();

            if (checkUser == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS"));
            }
            if (loginRequest.password == null || loginRequest.password.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PASSWORD_MISSING"));

            }

            if (checkUser.password != _commonFunction.ComputeSha256Hash(loginRequest.password))
            {
                return new JsonResult(new APIResponse("ERROR_PASSWORD_INCORRECT"));
            }

            if (checkUser.status != 1 || checkUser.customer_status != 1)
            {
                return new JsonResult(new APIResponse("ERROR_USER_LOCKED"));
            }

            string token = "";

            token = jwtAuth.BranchAuthentication(loginRequest.phone_number, loginRequest.password, Consts.USER_TYPE_CUSTOMER, (Guid)checkUser.customer_id);

            var settingObj = _context.Settingses.FirstOrDefault();
            Boolean is_review = false;
            if (settingObj != null && settingObj.is_review != null && settingObj.is_review == true)
            {
                is_review = true;
            }

            var loginResponse = new
            {
                token = token,
                user_id = checkUser.customer_id,
                username = checkUser.phone,
                full_name = checkUser.full_name,
                avatar = checkUser.avatar,
                is_have_secure_code = checkUser.is_have_secure_code,
                is_review = is_review,
                send_Notification = checkUser.send_Notification,
                send_Popup = checkUser.send_Popup,
                SMS_addPointSave = checkUser.SMS_addPointSave,
                SMS_addPointUse = checkUser.SMS_addPointSave
            };

            if (loginRequest.device_id != null && loginRequest.device_id.Length > 0)
            {
                if (loginRequest.device_id != checkUser.device_id)
                {
                    // Cập nhật device_id
                    var userObj = _context.Users.Where(x => x.customer_id == checkUser.customer_id).FirstOrDefault();
                    // Bắn Notification Firebase
                    string message = await _fcmNotification.SendNotification(checkUser.device_id, "LOGIN_OTHER_DEVICE", "Cảnh báo", "Tài khoản của bạn đã được đăng nhập trên thiết bị khác. Nếu không phải là bạn vui lòng kiểm tra lại tài khoản.", null);

                    userObj.device_id = loginRequest.device_id;

                    _context.SaveChanges();
                }
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "/api/app/auth/login",
                actions = "Đăng nhập với Tài khoản mới",
                application = "APP LOYALTY",
                content = "",
                functions = "Hệ thống",
                is_login = true,
                result_logging = "Thành công",
                user_created = "",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(loginResponse)) { StatusCode = 200 };
        }

        // API Check Mã giới thiệu
        [AllowAnonymous]
        [Route("checkShareCode")]
        [HttpPost]
        public async Task<JsonResult> CheckShareCode(LoginRequest loginRequest)
        {
            if (loginRequest.share_code == null || loginRequest.share_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_SHARE_CODE_MISSING"));
            }

            var checkUser = (from p in _context.Users
                             join c in _context.Customers on p.customer_id equals c.id into cs
                             from c in cs.DefaultIfEmpty()
                             join s in _context.Partners on p.partner_id equals s.id into ss
                             from s in ss.DefaultIfEmpty()
                             where p.share_code == loginRequest.share_code
                             select new
                             {
                                 customer_id = c != null ? c.id : s.id,
                                 full_name = c != null ? c.full_name : s.name,
                                 phone = c != null ? c.phone : s.phone,
                                 share_code = p.share_code
                             }).FirstOrDefault();

            if (checkUser == null)
            {
                return new JsonResult(new APIResponse("ERROR_SHARE_CODE_NOT_EXISTS"));
            }


            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            await _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "/api/app/auth/checkShareCode",
                actions = "Kiểm tra mã giới thiệu",
                application = "APP LOYALTY",
                content = "",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(checkUser)) { StatusCode = 200 };
        }

        // API Gửi mã xác nhận tạo mã bảo mật
        [Route("sendCodeCreateSecretKey")]
        [Authorize(Policy = "AppUser")]
        [HttpGet]
        public JsonResult AppSendCodeCreateSecretKey()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

            try
            {
                if (customerObj.time_otp_limit != null && customerObj.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.phone_number = customerObj.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "CUSTOMER_CREATE_SECRET_KEY";
                otp.date_limit = DateTime.Now.AddMinutes(1);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(customerObj.phone, "CashPlus: Ma OTP xac nhan tao ma bao mat tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/sendCodeCreateSecretKey",
                actions = "Gửi mail mã xác nhận tạo mật khẩu giao dịch",
                application = "APP LOYALTY",
                content = "Gửi mail mã xác nhận tạo mật khẩu giao dịch khách hàng: " + username.Value,
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
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult AppCreateSecretKey(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

            if (userObj.secret_key != null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_HAVE_SECRET_KEY")) { StatusCode = 200 };
            }

            if (req.new_secret_key == null || req.new_secret_key.Trim().Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MISSING")) { StatusCode = 200 };
            }

            if (req.new_secret_key.Length != 6)
            {
                return new JsonResult(new APIResponse("ERROR_NEW_SECRET_KEY_MUST_BE_LENGTH_EQUALS_6")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_CREATE_SECRET_KEY" && x.phone_number == customerObj.phone).FirstOrDefault();

            if (otp == null)
            {
                customerObj.count_otp_fail = customerObj.count_otp_fail != null ? customerObj.count_otp_fail + 1 : 1;
                if(customerObj.count_otp_fail >= 3){
                   customerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                   customerObj.count_otp_fail = 0;
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
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/createSecretKey",
                actions = "Tạo mã bảo mật",
                application = "APP LOYALTY",
                content = "Tạo mã bảo mật khách hàng: " + username.Value,
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
        [Authorize(Policy = "AppUser")]
        [HttpGet]
        public JsonResult AppSendCodeChangeSecretKey()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

            try
            {

                if (customerObj.time_otp_limit != null && customerObj.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.phone_number = customerObj.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "CUSTOMER_CHANGE_SECRET_KEY";
                otp.date_limit = DateTime.Now.AddMinutes(1);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(customerObj.phone, "CashPlus: Ma OTP xac nhan thay doi ma bao mat tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/sendCodeForgetSecretKey",
                actions = "Gửi mail mã xác nhận quên mật khẩu giao dịch",
                application = "APP LOYALTY",
                content = "Gửi mail mã xác nhận quên mật khẩu giao dịch khách hàng: " + username.Value,
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
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult AppChangeSecretKey(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }
            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_CHANGE_SECRET_KEY" && x.phone_number == customerObj.phone).FirstOrDefault();

            if (otp == null)
            {
                customerObj.count_otp_fail = customerObj.count_otp_fail != null ? customerObj.count_otp_fail + 1 : 1;
                if (customerObj.count_otp_fail >= 3)
                {
                    customerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    customerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

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
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/changeSecretKey",
                actions = "Đổi mã bảo mật",
                application = "APP LOYALTY",
                content = "Đổi mã bảo mật khách hàng: " + username.Value,
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
        [Authorize(Policy = "AppUser")]
        [HttpGet]
        public JsonResult AppSendCodeForgetSecretKey()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

            try
            {

                if (customerObj.time_otp_limit != null &&  customerObj.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.phone_number = customerObj.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "CUSTOMER_FORGET_SECRET_KEY";
                otp.date_limit = DateTime.Now.AddMinutes(1);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                _emailSender.SendSms(customerObj.phone, "CashPlus: Ma OTP xac nhan thay doi ma bao mat tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/sendCodeForgetSecretKey",
                actions = "Gửi mail mã xác nhận quên mật khẩu giao dịch",
                application = "APP LOYALTY",
                content = "Gửi mail mã xác nhận quên mật khẩu giao dịch khách hàng: " + username.Value,
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
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult AppRenewSecretKey(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

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

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_FORGET_SECRET_KEY").FirstOrDefault();

            if (otp == null)
            {
                customerObj.count_otp_fail = customerObj.count_otp_fail != null ? customerObj.count_otp_fail + 1 : 1;
                if (customerObj.count_otp_fail >= 3)
                {
                   customerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                   customerObj.count_otp_fail = 0;
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
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/renewSecretKey",
                actions = "Tạo mật khẩu giao dịch mới khi quên mật khẩu giao dịch",
                application = "APP LOYALTY",
                content = "Tạo mật khẩu giao dịch mới khi quên mật khẩu giao dịch khách hàng: " + username.Value,
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
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult AppUpdateCustomerInfo(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();
            var userObj = _context.Users.Where(x => x.customer_id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (userObj != null)
            {
                userObj.send_Notification = req.send_Notification != null ? req.send_Notification : false;
                userObj.send_Popup = req.send_Popup != null ? req.send_Popup : false;
                userObj.SMS_addPointSave = req.SMS_addPointSave != null ? req.SMS_addPointSave : false;
                userObj.SMS_addPointUse = req.SMS_addPointUse != null ? req.SMS_addPointUse : false;
            }

            if (req.full_name == null)
            {
                return new JsonResult(new APIResponse("ERROR_PROVINCE_ID_MISSING")) { StatusCode = 200 };
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                customerObj.full_name = req.full_name;
                customerObj.email = req.email;
                customerObj.address = req.address;

                if (req.birth_date != null && req.birth_date.Length == 10)
                {
                    customerObj.birth_date = _commonFunction.convertStringSortToDate(req.birth_date);
                }
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
                api_name = "api/app/auth/updateCusInfo",
                actions = "Cập nhật thông tin khách hàng",
                application = "APP LOYALTY",
                content = "Cập nhật thông tin khách hàng: " + username.Value,
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
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult AppUpdateCustomerAvatar(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (req.avatar == null || req.avatar.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_LOGO_MISSING")) { StatusCode = 200 };
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                customerObj.avatar = req.avatar;

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
                api_name = "api/app/auth/updateCusAvatar",
                actions = "Cập nhật ảnh đại diện khách hàng",
                application = "APP LOYALTY",
                content = "Cập nhật ảnh đại diện khách hàng: " + username.Value,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Đổi mật khẩu Khách hàng
        [Route("changePass")]
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult AppChangePassword(PasswordRequest pwdRequest)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            var user = _context.Users.Where(x => x.username == customerObj.phone && x.is_customer == true).FirstOrDefault();

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

            try
            {
                user.password = _commonFunction.ComputeSha256Hash(pwdRequest.new_password);
                _context.SaveChanges();

                // Gửi email
                try
                {
                    if (customerObj.email != null)
                    {
                        string subjectEmail = "[CashPlus] - Đổi mật khẩu thành công";
                        string mail_to = customerObj.email;
                        string message = "<p>Xin chào " + customerObj.full_name + ",<p>";
                        message += "<p>Tài khoản của bạn " + username.Value + " đã được thay đổi mật khẩu thành công.</p>";
                        message += "<p>Nếu đây không phải là yêu cầu của bạn, vui lòng liên hệ bộ phận chăm sóc khách hàng.</p>";
                        message += "<p>Trân trọng!</p>";
                        message += "<br/>";
                        message += "<p>@2023 ATS Group</p>";

                        _emailSender.SendEmailAsync(mail_to, subjectEmail, message);
                    }
                }
                catch (Exception ex3)
                {

                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse("ERROR_CHANGE_PASS_FAIL")) { StatusCode = 200 };
            }
            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/changePass",
                actions = "Đổi mật khẩu",
                application = "APP LOYALTY",
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
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult BranchLogout()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            var userObj = _context.Users.Where(x => x.is_customer == true && x.username == username.Value).FirstOrDefault();

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
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/logout",
                actions = "Đăng xuất",
                application = "APP LOYALTY",
                content = "Đăng xuất",
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
        [Route("sendCodeForgetPassword")]
        [HttpPost]
        public async Task<JsonResult> AppSendCodeForgetPassword(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            if (req.phone == null || req.phone.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (req.phone.Contains("+84"))
            {
                req.phone = req.phone.Replace("+84", "0");
            }

            var checkPhone = _context.Customers.Where(x => x.phone == req.phone).FirstOrDefault();

            if (checkPhone == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_NOT_REGISTER")) { StatusCode = 200 };
            }

            var userStatus = _context.Users.Where(x => x.is_customer == true && x.customer_id == checkPhone.id).Select(x => x.status).FirstOrDefault();

            if (userStatus == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_NOT_REGISTER")) { StatusCode = 200 };

            }
            if (checkPhone.status != 1 || userStatus != 1)
            {
                return new JsonResult(new APIResponse("ERROR_USER_LOCK")) { StatusCode = 200 };
            }

            try
            {

                if ( checkPhone.time_otp_limit != null && checkPhone.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.object_name = req.phone;
                otp.phone_number = req.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "CUSTOMER_FORGET_PASSWORD";
                otp.date_limit = DateTime.Now.AddMinutes(1);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                await _emailSender.SendSms(req.phone, "CashPlus: Ma OTP xac nhan dat lai mat khau tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
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
                api_name = "api/app/auth/sendCodeForgetPassword",
                actions = "Gửi mail mã xác nhận quên mật khẩu khách hàng",
                application = "APP LOYALTY",
                content = "Gửi mail mã xác nhận quên mật khẩu khách hàng: " + req.phone,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = req.phone,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Confirm mã OTP quên mật khẩu
        [Route("confirmCusForgetPassword")]
        [HttpPost]
        public JsonResult AppConfirmCustomerForgetPassword(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            if (req.phone == null || req.phone.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (req.phone.Contains("+84"))
            {
                req.phone = req.phone.Replace("+84", "0");
            }

            var customerObj = _context.Customers.Where(x => x.phone == req.phone).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_NOT_REGISTER")) { StatusCode = 200 };
            }

            if (req.otp_code == null || req.otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }
            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();
            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_FORGET_PASSWORD" && x.phone_number == req.phone).FirstOrDefault();

            if (otp == null)
            {
                customerObj.count_otp_fail = customerObj.count_otp_fail != null ? customerObj.count_otp_fail + 1 : 1;
                if(customerObj.count_otp_fail >= 3){
                    customerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    customerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Cập nhật mật khẩu khi quên
        [Route("updateCusForgetPassword")]
        [HttpPost]
        public JsonResult AppUpdateCustomerForgetPassword(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            if (req.phone == null || req.phone.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (req.phone.Contains("+84"))
            {
                req.phone = req.phone.Replace("+84", "0");
            }

            var customerObj = _context.Customers.Where(x => x.phone == req.phone).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_NOT_REGISTER")) { StatusCode = 200 };
            }

            if (req.password == null || req.password.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PASSWORD_MISSING")) { StatusCode = 200 };
            }

            //// Check password
            //Regex r = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$");

            //if (r.IsMatch(req.password) == false)
            //{
            //    return new JsonResult(new APIResponse(Messages.ERROR_PASSWORD_INCORRECT_PATTERN));
            //}

            if (req.otp_code == null || req.otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_FORGET_PASSWORD" && x.phone_number == req.phone).FirstOrDefault();
            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();
            if (otp == null)
            {
                customerObj.count_otp_fail = customerObj.count_otp_fail != null ? customerObj.count_otp_fail + 1 : 1;
                if(customerObj.count_otp_fail >= 3){
                    customerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    customerObj.count_otp_fail = 0;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            var userInfo = _context.Users.Where(x => x.username == req.phone && x.is_customer == true).FirstOrDefault();

            if (userInfo == null)
            {
                return new JsonResult(new APIResponse("ERROR_USER_NOT_EXISTS")) { StatusCode = 200 };
            }
            var transaction = _context.Database.BeginTransaction();
            try
            {
                userInfo.password = _commonFunction.ComputeSha256Hash(req.password);

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
                api_name = "api/app/auth/updateCusPhone",
                actions = "Cập nhật Số điện thoại khách hàng",
                application = "APP LOYALTY",
                content = "Cập nhật Số điện thoại khách hàng: " + req.phone,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = req.phone,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Gửi mã xác nhận đổi sđt
        [AllowAnonymous]
        [Route("sendCodeChangePhone")]
        [HttpPost]
        public async Task<JsonResult> AppSendCodeChangePhone(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            if (req.phone == null || req.phone.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (req.phone.Contains("+84"))
            {
                req.phone = req.phone.Replace("+84", "0");
            }

            var checkPhone = _context.Customers.Where(x => x.phone == req.phone).FirstOrDefault();

            if (checkPhone == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_NOT_REGISTER")) { StatusCode = 200 };
            }

            try
            {
                if (checkPhone.time_otp_limit != null && checkPhone.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.object_name = req.phone;
                otp.phone_number = req.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "CUSTOMER_CHANGE_PHONE";
                otp.date_limit = DateTime.Now.AddMinutes(1);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                await _emailSender.SendSms(req.phone, "CashPlus: Ma OTP xac nhan thay doi so dien thoai tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
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
                api_name = "api/app/auth/sendCodeForgetPassword",
                actions = "Gửi mail mã xác nhận quên mật khẩu khách hàng",
                application = "APP LOYALTY",
                content = "Gửi mail mã xác nhận quên mật khẩu khách hàng: " + req.phone,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = req.phone,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Confirm mã OTP đổi đt
        [Route("confirmCodeChangePhone")]
        [HttpPost]
        public JsonResult AppConfirmCodeChangePhone(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            if (req.phone == null || req.phone.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (req.phone.Contains("+84"))
            {
                req.phone = req.phone.Replace("+84", "0");
            }

            var checkPhone = _context.Customers.Where(x => x.phone == req.phone).FirstOrDefault();

            if (checkPhone == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_NOT_REGISTER")) { StatusCode = 200 };
            }

            if (req.otp_code == null || req.otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_CHANGE_PHONE" && x.phone_number == req.phone).FirstOrDefault();

            if (otp == null)
            {
                checkPhone.count_otp_fail = checkPhone.count_otp_fail != null ? checkPhone.count_otp_fail + 1 : 1;
                if (checkPhone.count_otp_fail >= 3)
                {
                    checkPhone.status = 2;
                }
                _context.SaveChanges(); 
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Gửi mã xác nhận đổi sđt mới
        [AllowAnonymous]
        [Route("sendCodeChangePhoneNew")]
        [HttpPost]
        public async Task<JsonResult> AppSendCodeChangePhoneNew(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            if (req.phone == null || req.phone.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (req.phone.Contains("+84"))
            {
                req.phone = req.phone.Replace("+84", "0");
            }

            var checkPhone = _context.Customers.Where(x => x.phone == req.phone).FirstOrDefault();

            if (checkPhone != null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_EXISTS")) { StatusCode = 200 };
            }

            try
            {
                if (checkPhone.time_otp_limit != null && checkPhone.time_otp_limit > DateTime.Now)
                {
                    return new JsonResult(new APIResponse("ERROR_OTP_LIMIT")) { StatusCode = 200 };
                }
                Random rnd = new Random();
                int code = rnd.Next(100000, 999999);

                var otp = new OTPTransaction();
                otp.otp_code = code.ToString();
                otp.object_name = req.phone;
                otp.phone_number = req.phone;
                otp.date_created = DateTime.Now;
                otp.object_type = "CUSTOMER_CHANGE_PHONE_NEW";
                otp.date_limit = DateTime.Now.AddMinutes(1);
                _context.OTPTransactions.Add(otp);
                _context.SaveChanges();

                await _emailSender.SendSms(req.phone, "CashPlus: Ma OTP xac nhan thay doi so dien thoai tai CashPlus Tieu Dung %26 Hoan Tien cua Quy khach la: <%23> code " + code.ToString());
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
                api_name = "api/app/auth/sendCodeForgetPassword",
                actions = "Gửi mail mã xác nhận quên mật khẩu khách hàng",
                application = "APP LOYALTY",
                content = "Gửi mail mã xác nhận quên mật khẩu khách hàng: " + req.phone,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = req.phone,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // API Cập nhật số điện thoại mới
        [Route("updateCusPhoneNumber")]
        [HttpPost]
        public JsonResult AppUpdateCustomerPhoneNumer(AppCusInfoRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();

            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }

            if (req.phone == null || req.phone.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (req.phone.Contains("+84"))
            {
                req.phone = req.phone.Replace("+84", "0");
            }

            var checkPhone = _context.Customers.Where(x => x.phone == req.phone).FirstOrDefault();

            if (checkPhone != null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_EXIST")) { StatusCode = 200 };
            }

            if (req.old_otp_code == null || req.old_otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OLD_OTP_CODE_MISSING")) { StatusCode = 200 };
            }
            var userInfo = _context.Users.Where(x => x.customer_id == customerObj.id && x.is_customer == true).FirstOrDefault();

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.old_otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_CHANGE_PHONE" && x.phone_number == req.phone).FirstOrDefault();

            if (otp == null)
            {
                checkPhone.count_otp_fail = checkPhone.count_otp_fail != null ? checkPhone.count_otp_fail + 1 : 1;
                if(checkPhone.count_otp_fail >= 3){
                    checkPhone.status = 2;
                    userInfo.status = 2;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }

            if (req.otp_code == null || req.otp_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING")) { StatusCode = 200 };
            }

            var otp2 = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_CHANGE_PHONE_NEW" && x.phone_number == req.phone).FirstOrDefault();

            if (otp2 == null)
            {
                checkPhone.count_otp_fail = checkPhone.count_otp_fail != null ? checkPhone.count_otp_fail + 1 : 1;
                if (checkPhone.count_otp_fail >= 3)
                {
                    checkPhone.status = 2;
                    userInfo.status = 2;
                }
                _context.SaveChanges();
                return new JsonResult(new APIResponse("ERROR_OTP_CODE_INCORRECT")) { StatusCode = 200 };
            }
            if (userInfo == null)
            {
                return new JsonResult(new APIResponse("ERROR_USER_NOT_EXISTS")) { StatusCode = 200 };
            }
            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Cập nhật tài khoản
                userInfo.username = req.phone;
                _context.SaveChanges();

                // Cập nhât số điện thoại khách hàng
                customerObj.phone = req.phone;
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
                api_name = "api/app/auth/updateCusPhone",
                actions = "Cập nhật Số điện thoại khách hàng",
                application = "APP LOYALTY",
                content = "Cập nhật Số điện thoại khách hàng: " + req.phone,
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = req.phone,
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        // // API Gửi OTP Test
        // [AllowAnonymous]
        // [Route("sendOTPByPhone")]
        // [HttpPost]
        // public JsonResult SendOTPByPhone(LoginRequest loginRequest)
        // {
        //     if (loginRequest.phone_number == null || loginRequest.phone_number.Length == 0)
        //     {
        //         return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_MISSING"));
        //     }

        //     if (loginRequest.phone_number.Contains("+84"))
        //     {
        //         loginRequest.phone_number = loginRequest.phone_number.Replace("+84", "0");
        //     }


        //     Random rnd = new Random();
        //     int code = rnd.Next(100000, 999999);

        //     var otp = new OTPTransaction();
        //     otp.otp_code = code.ToString();
        //     otp.object_name = loginRequest.phone_number;
        //     otp.date_created = DateTime.Now;
        //     otp.object_type = "OTP_TEST";
        //     otp.date_limit = DateTime.Now.AddMinutes(1);
        //     _context.OTPTransactions.Add(otp);
        //     _context.SaveChanges();

        //     _emailSender.SendSms(loginRequest.phone_number, "CashPlus: Ma OTP xac nhan thay doi so dien thoai tai https://CashPlus.vn cua Quy khach la: " + code.ToString());

        //     // Ghi log
        //     var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
        //     _loggingHelpers.insertLogging(new LoggingRequest
        //     {
        //         user_type = Consts.USER_TYPE_CUSTOMER,
        //         is_call_api = true,
        //         api_name = "/api/app/auth/sendOTPByPhone",
        //         actions = "Gửi OTP Test",
        //         application = "APP LOYALTY",
        //         content = "",
        //         functions = "Hệ thống",
        //         is_login = true,
        //         result_logging = "Thành công",
        //         user_created = "",
        //         IP = remoteIP.ToString()
        //     });
        //     return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        // }

        // [AllowAnonymous]
        // [Route("checkOTPByPhone")]
        // [HttpPost]
        // public JsonResult CheckOTPByPhone(LoginRequest loginRequest)
        // {
        //     if (loginRequest.phone_number == null || loginRequest.phone_number.Length == 0)
        //     {
        //         return new JsonResult(new APIResponse("ERROR_PHONE_NUMBER_MISSING"));
        //     }

        //     if (loginRequest.phone_number.Contains("+84"))
        //     {
        //         loginRequest.phone_number = loginRequest.phone_number.Replace("+84", "0");
        //     }

        //     if (loginRequest.otp_code == null || loginRequest.otp_code.Length == 0)
        //     {
        //         return new JsonResult(new APIResponse("ERROR_OTP_CODE_MISSING"));
        //     }

        //     var otp = _context.OTPTransactions.Where(x => x.otp_code == loginRequest.otp_code && x.date_limit > DateTime.Now && x.object_type == "OTP_TEST" && x.object_name == loginRequest.phone_number).FirstOrDefault();

        //     if (otp == null)
        //     {
        //         return new JsonResult(new APIResponse(Messages.ERROR_OTP_CODE_INCORRECT)) { StatusCode = 200 };
        //     }

        //     // Ghi log
        //     var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
        //     _loggingHelpers.insertLogging(new LoggingRequest
        //     {
        //         user_type = Consts.USER_TYPE_CUSTOMER,
        //         is_call_api = true,
        //         api_name = "/api/app/auth/checkOTPByPhone",
        //         actions = "Gửi OTP Test",
        //         application = "APP LOYALTY",
        //         content = "",
        //         functions = "Hệ thống",
        //         is_login = true,
        //         result_logging = "Thành công",
        //         user_created = "",
        //         IP = remoteIP.ToString()
        //     });
        //     return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        // }

        // API Gửi mã xác nhận đổi mã bảo mật
        [Route("getPolicy")]
        [AllowAnonymous]
        [HttpGet]
        public JsonResult AppGetPolicy()
        {

            var dataReturn = _context.Settingses.Select(x => x.policy).FirstOrDefault();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/getPolicy",
                actions = "Lấy thông tin chính sách",
                application = "APP LOYALTY",
                content = "Lấy thông tin chính sách",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(new
            {
                data = dataReturn
            }))
            { StatusCode = 200 };
        }

        // API Update is_reivew
        [Route("updateReview")]
        [AllowAnonymous]
        [HttpGet]
        public JsonResult UpdateReview()
        {

            var dataReturn = _context.Settingses.FirstOrDefault();
            if (dataReturn.is_review == null || dataReturn.is_review == false)
            {
                dataReturn.is_review = true;
            }
            else
            {
                dataReturn.is_review = false;
            }

            _context.SaveChanges();

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/updateReview",
                actions = "Cập nhật trạng thái review app",
                application = "APP LOYALTY",
                content = "Cập nhật trạng thái review app",
                functions = "Hệ thống",
                is_login = false,
                result_logging = "Thành công",
                user_created = "Anonymous",
                IP = remoteIP.ToString()
            });
            return new JsonResult(new APIResponse(200))
            { StatusCode = 200 };
        }

        // API Check mã cũ
        [Route("checkOldSecretKey")]
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult checkOldSecretKey(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }

            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

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
        [Route("changeSecretKeyv2")]
        [Authorize(Policy = "AppUser")]
        [HttpPost]
        public JsonResult changeSecretKeyv2(SecretKeyRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var customerToken = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == Guid.Parse(customerToken.Value)).FirstOrDefault();

            if (customerObj == null)
            {
                return new JsonResult(new APIResponse("ERROR_CUSTOMER_NOT_EXISTS")) { StatusCode = 200 };
            }
            var userObj = _context.Users.Where(x => x.is_customer == true && x.customer_id == customerObj.id).FirstOrDefault();

            var otp = _context.OTPTransactions.Where(x => x.otp_code == req.otp_code && x.date_limit > DateTime.Now && x.object_type == "CUSTOMER_CHANGE_SECRET_KEY" && x.phone_number == customerObj.phone).FirstOrDefault();

            if (otp == null)
            {
                customerObj.count_otp_fail = customerObj.count_otp_fail != null ? customerObj.count_otp_fail + 1 : 1;
                if (customerObj.count_otp_fail >= 3)
                {
                    customerObj.time_otp_limit = DateTime.Now.AddMinutes(30);
                    customerObj.count_otp_fail = 0;
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
                user_type = Consts.USER_TYPE_CUSTOMER,
                is_call_api = true,
                api_name = "api/app/auth/changeSecretKey",
                actions = "Đổi mã bảo mật",
                application = "APP LOYALTY",
                content = "Đổi mã bảo mật khách hàng: " + username.Value,
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
