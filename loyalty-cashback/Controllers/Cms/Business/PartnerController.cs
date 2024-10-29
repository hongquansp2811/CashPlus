using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Interfaces;
using LOYALTY.Data;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using System.Security.Claims;
using System.IO;
using ClosedXML.Excel;
using LOYALTY.PaymentGate;
using System.Net.Mail;
using System.Net;
using MailKit.Security;
using MimeKit;

namespace LOYALTY.Controllers
{
    [Route("api/partner")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class PartnerController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IPartner _app;
        private readonly ILoggingHelpers _logging;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly BKTransaction _bkTransaction;
        private static string functionCode = "cms_partner";
        public PartnerController(IConfiguration configuration, IPartner app, ILoggingHelpers logging, IEmailSender emailSender, BKTransaction bkTransaction, LOYALTYContext context)
        {
            _configuration = configuration;
            this._app = app;
            this._logging = logging;
            _context = context;
            _emailSender = emailSender;
            _bkTransaction = bkTransaction;
        }

        [Route("list")]
        [HttpPost]
        public JsonResult GetList(PartnerRequest request)
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
                api_name = "api/partner/list",
                actions = "Danh sách đối tác",
                application = "WEB ADMIN",
                content = "Danh sách đối tác",
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
                api_name = "api/partner/" + id,
                actions = "Chi tiết Đối tác",
                application = "WEB ADMIN",
                content = "Chi tiết Đối tác",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [HttpGet("getBalance/{partner_id}")]
        public JsonResult GetBalancePartner(Guid partner_id)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            APIResponse data = _app.getBalance(partner_id);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/partner/getBalance/" + partner_id,
                actions = "Chi tiết Đối tác",
                application = "WEB ADMIN",
                content = "Chi tiết Đối tác",
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
        public JsonResult Create(PartnerRequest request)
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
                api_name = "api/partner/create",
                actions = "Thêm mới Đối tác",
                application = "WEB ADMIN",
                content = "Thêm mới Đối tác",
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
        public JsonResult Update(PartnerRequest request)
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
                api_name = "api/partner/update",
                actions = "Cập nhật Đối tác",
                application = "WEB ADMIN",
                content = "Cập nhật Đối tác",
                functions = "Danh mục",
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Delete))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.delete(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/partner/delete",
                actions = "Xóa Đối tác",
                application = "WEB ADMIN",
                content = "Xóa Đối tác",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("lock")]
        [HttpPost]
        public JsonResult Lock(DeleteGuidRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();

            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Other))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.lockAccount(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/partner/lock",
                actions = "Khóa Đối tác",
                application = "WEB ADMIN",
                content = "Khóa Đối tác",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("unlock")]
        [HttpPost]
        public JsonResult Unlock(DeleteGuidRequest request)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Other))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.unlockAccount(request);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/partner/unlock",
                actions = "Mở khóa Đối tác",
                application = "WEB ADMIN",
                content = "Mở khóa Đối tác",
                functions = "Danh mục",
                is_login = false,
                result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("changePassword")]
        [HttpPost]
        public JsonResult changePassword(PasswordRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Other))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }

            APIResponse data = _app.changePassword(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/partner/changePassword",
                    actions = "Đổi mật khẩu cửa hàng",
                    application = "WEB ADMIN",
                    content = "Đổi mật khẩu cửa hàng",
                    functions = "Nghiệp vụ",
                    is_login = false,
                    result_logging = data.code == "200" ? "Thành công" : "Thất bại",
                    user_created = username.Value,
                    IP = remoteIP.ToString()
                });
            }
            return new JsonResult(data) { StatusCode = 200 };
        }

        [Route("getListAccumulatePointOrder")]
        [HttpPost]
        public JsonResult getListAccumulatePointOrder(AccumulatePointOrderRequest req)
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getListAccumulatePointOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/partner/getListAccumulatePointOrder",
                    actions = "Danh sách chứng từ tích điểm",
                    application = "WEB ADMIN",
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getListChangePointOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/partner/getListChangePointOrder",
                    actions = "Danh sách chứng từ đổi điểm",
                    application = "WEB ADMIN",
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getListPartnerOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/partner/getListPartnerOrder",
                    actions = "Danh sách chứng từ mua hàng",
                    application = "WEB ADMIN",
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getListAddPointOrder(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/partner/getListAddPointOrder",
                    actions = "Danh sách nạp điểm",
                    application = "WEB ADMIN",
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getListAccumulatePointOrderRating(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/partner/getListAccumulatePointOrderRating",
                    actions = "Danh sách đánh giá đơn hàng",
                    application = "WEB ADMIN",
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            APIResponse data = _app.getListTeam(req);
            // Ghi log
            if (data.code == "200")
            {
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/partner/getListTeam",
                    actions = "Danh sách đội nhóm",
                    application = "WEB ADMIN",
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

        [HttpPost("importPartner")]
        public JsonResult ImportPartner(IFormFile file)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, functionCode, (int)Enums.ActionType.Import))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            if (file == null || file.Length <= 0)
            {
                return new JsonResult(new APIResponse("ERROR_FILE_NOT_FOUND")) { StatusCode = 200 };
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new APIResponse("ERROR_FILE_NOT_SUPPORT")) { StatusCode = 200 };
            }

            int totalRowImport = 0;
            var rowFails = new List<RowFailModel>();

            try
            {
                var fileextension = Path.GetExtension(file.FileName);
                var filename = Guid.NewGuid().ToString() + fileextension;

                string uploadFolder = "";
                uploadFolder = _configuration.GetSection("Config")["Directory"] + "\\loyalty";
                var filepath = Path.Combine(uploadFolder, filename);
                using (FileStream fs = System.IO.File.Create(filepath))
                {
                    file.CopyTo(fs);
                }
                int rowno = 1;
                XLWorkbook workbook = XLWorkbook.OpenFromTemplate(filepath);
                var sheets = workbook.Worksheets.First();
                var rows = sheets.Rows().ToList();
                List<PartnerRequest> list_imports = new List<PartnerRequest>();
                foreach (var row in rows)
                {
                    if (rowno != 1)
                    {
                        var test = row.Cell(1).Value.ToString();
                        if (string.IsNullOrWhiteSpace(test) || string.IsNullOrEmpty(test))
                        {
                            break;
                        }

                        PartnerRequest data = new PartnerRequest();

                        data.code = row.Cell(3).Value.ToString();
                        data.name = row.Cell(4).Value.ToString();
                        data.email = row.Cell(5).Value.ToString();
                        data.phone = row.Cell(6).Value.ToString();
                        data.store_owner = row.Cell(7).Value.ToString();
                        data.start_hour = row.Cell(8).Value.ToString();
                        data.end_hour = row.Cell(9).Value.ToString();
                        data.working_day = row.Cell(10).Value.ToString();
                        data.tax_code = row.Cell(11).Value.ToString();
                        data.description = row.Cell(12).Value.ToString();
                        data.username = row.Cell(13).Value.ToString();
                        data.password = row.Cell(14).Value.ToString();
                        data.address = row.Cell(18).Value.ToString();
                        data.latitude = row.Cell(19).Value.ToString();
                        data.longtitude = row.Cell(20).Value.ToString();
                        // Loại dịch vụ
                        var service_type_name = row.Cell(1).Value.ToString();
                        var service_type_id = _context.ServiceTypes.Where(x => x.name.ToLower() == service_type_name.ToLower()).Select(x => x.id).FirstOrDefault();
                        if (service_type_id == null)
                        {
                            continue;
                        }
                        data.service_type_id = service_type_id;

                        // Loại cửa hàng
                        var store_type_name = row.Cell(2).Value.ToString();
                        var store_type_id = _context.OtherLists.Where(x => x.name.ToLower() == store_type_name.ToLower() && x.type == 5).Select(x => x.id).FirstOrDefault();
                        if (store_type_id == null)
                        {
                            continue;
                        }
                        data.store_type_id = store_type_id;

                        // Tỉnh/Thành phố
                        var province_name = row.Cell(15).Value.ToString();
                        var province_id = _context.Provinces.Where(x => x.name.ToLower() == province_name.ToLower() && x.types == 1).Select(x => x.id).FirstOrDefault();
                        if (province_id == null)
                        {
                            continue;
                        }
                        data.province_id = province_id;

                        // Quận/Huyên
                        var district_name = row.Cell(16).Value.ToString();
                        var district_id = _context.Provinces.Where(x => x.name.ToLower() == district_name.ToLower() && x.types == 2 && x.parent_id == province_id).Select(x => x.id).FirstOrDefault();
                        if (district_id == null)
                        {
                            continue;
                        }
                        data.district_id = district_id;

                        // Xã phường
                        var ward_name = row.Cell(17).Value.ToString();
                        var ward_id = _context.Provinces.Where(x => x.name.ToLower() == ward_name.ToLower() && x.types == 3 && x.parent_id == district_id).Select(x => x.id).FirstOrDefault();
                        if (ward_id == null)
                        {
                            continue;
                        }
                        data.ward_id = ward_id;

                        list_imports.Add(data);
                    }
                    else
                    {
                        rowno = 2;
                    }
                }

                var total_success = 0;
                var total_fail = 0;
                var list_fail = new List<RowFailModel>();

                if (list_imports.Count > 0)
                {
                    for (int i = 0; i < list_imports.Count; i++)
                    {
                        APIResponse res1 = _app.create(list_imports[i], "administrator");

                        if (res1.code == "200")
                        {
                            total_success += 1;
                        }
                        else
                        {
                            total_fail += 1;
                            var rowFail = new RowFailModel();
                            rowFail.row = i + 1;
                            rowFail.err_messages = res1.error;
                            list_fail.Add(rowFail);
                        }
                    }

                    var dataReturn = new
                    {
                        total_success = total_success,
                        total_fail = total_fail,
                        list_fail = list_fail
                    };

                    // Ghi log
                    var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
                    _logging.insertLogging(new LoggingRequest
                    {
                        user_type = Consts.USER_TYPE_WEB_ADMIN,
                        is_call_api = true,
                        api_name = "api/partner/importPartner",
                        actions = "Import cửa hàng",
                        application = "WEB ADMIN",
                        content = "Import cửa hàng",
                        functions = "Danh mục",
                        is_login = false,
                        result_logging = "Thành công",
                        user_created = "administrator",
                        IP = remoteIP.ToString()
                    });
                    return new JsonResult(new APIResponse(dataReturn)) { StatusCode = 200 };
                }
                else
                {
                    return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
        }

        public class RowFailModel
        {
            public int row { get; set; }
            public string err_messages { get; set; }
        }

        public class EnCodePartnerRes
        {
            public string? bk_partner_code { get; set; }
            public string? bk_merchant_id { get; set; }
            public string? bk_email { get; set; }
            public string? bk_password { get; set; }
        }

        // mã hóa thông tin đối tác
        [Route("EnCodePartner")]
        [HttpPost]
        public JsonResult EnCodePartner(EnCodePartnerRes req)
        {
            try
            {

                string passPhrase = "Pas5pr@se";
                string saltValue = "s@1tValue";
                string hashAlgorithm = "SHA1";
                int passwordIterations = 2;
                string initVector = "@CSS@CSS@CSS@CSS";
                int keySize = 256;

                EnCodePartnerRes lst = new EnCodePartnerRes();

                lst.bk_partner_code = EncryptData.Encrypt(req.bk_partner_code, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
                lst.bk_merchant_id = EncryptData.Encrypt(req.bk_merchant_id, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
                lst.bk_email = EncryptData.Encrypt(req.bk_email, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
                lst.bk_password = EncryptData.Encrypt(req.bk_password, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);

                //var deCode1 = EncryptData.Decrypt(lst.bk_partner_code, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
                //string deCode2 = EncryptData.Decrypt(lst.bk_merchant_id, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
                //var deCode3 = EncryptData.Decrypt(lst.bk_email, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
                //var deCode4 = EncryptData.Decrypt(lst.bk_password, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);

                return new JsonResult(new APIResponse(lst)) { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(ex)) { StatusCode = 500 };
            }
        }


        public class sendKeyToBkReq
        {
            public string Public_key { get; set; }
            public string partnerName { get; set; }
            public string partnerCode { get; set; }
        }

        [Route("sendKeyToBK")]
        [HttpPost]
        public JsonResult sendKeyToBK(sendKeyToBkReq req)
        {
            try
            {
                string passPhrase = "Pas5pr@se";
                string saltValue = "s@1tValue";
                string hashAlgorithm = "SHA1";
                int passwordIterations = 2;
                string initVector = "@CSS@CSS@CSS@CSS";
                int keySize = 256;
                var partnerCodeDe = EncryptData.Decrypt(req.partnerCode, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);

                List<string> mail_to = new List<string>();
                var mail1 = BKConsts.EMAIL_BK.Split(",");
                foreach (var i in mail1)
                {
                    mail_to.Add(i.Trim());
                }
                string subjectEmail = "ATS gửi Public key đối tác " + req.partnerName + " _ " + partnerCodeDe;
                string messages = "<p>" + req.partnerName + " _ " + partnerCodeDe + "</p>";
                messages += "<p> Tên đối tác: " + req.partnerName + "</p>";
                messages += "<p> Mã đối tác: " + partnerCodeDe + "</p>";
                messages += "<p> Public key: " + req.Public_key + "</p>";
                messages += "<p>Trân trọng!</p>";
                messages += "<br/>";
                messages += "<p>@2023 ATS Group</p>";

                string filePath = _configuration["Config:Directory"] + "\\" + partnerCodeDe + ".pem";
                string data = req.Public_key;
                System.IO.File.WriteAllText(filePath, data);

                var host = _configuration["Email:host"];
                var user = _configuration["Email:user"];
                var password = _configuration["Email:password"];

                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(user));
                for (int i = 0; i < mail_to.Count; i++)
                {
                    message.To.Add(MailboxAddress.Parse(mail_to[i]));
                }
                message.Subject = subjectEmail;
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = messages };
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var attachment = new MimePart("application", "octet-stream")
                    {
                        Content = new MimeContent(stream),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(filePath)
                    };
                    var multipart = new Multipart("mixed");
                    multipart.Add(message.Body);
                    multipart.Add(attachment);
                    message.Body = multipart;
                    try
                    {
                        using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                        {
                            smtp.Connect(host, 25, SecureSocketOptions.StartTlsWhenAvailable);
                            smtp.Authenticate(user, password);
                            smtp.Send(message);
                            smtp.Disconnect(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult(new APIResponse(ex.ToString())) { StatusCode = 500 };
                    }
                }
                System.IO.File.Delete(filePath);
                return new JsonResult(new APIResponse(200, "Gửi email thành công"));
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(ex.ToString())) { StatusCode = 500 };
            }
        }


        [Route("check/{partner_id}")]
        [HttpGet]
        public JsonResult check(Guid? partner_id)
        {
            try
            {
                var partnerObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

                var BankCode = _context.Banks.Where(p => p.id == partnerObj.bk_bank_id).Select(p => p.bank_code).FirstOrDefault();
                var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;

                CheckVerifyConnect response = _bkTransaction.checkVerifyConnect(partnerObj.bk_partner_code, BankCode, partnerObj.bk_bank_no, partnerObj.RSA_privateKey, remoteIP.ToString());

                if (response.ResponseCode == 200)
                {
                    return new JsonResult(new APIResponse(200, "Đấu nối thành công"));
                }
                else
                {
                    return new JsonResult(new APIResponse(response.ResponseMessage)) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(ex)) { StatusCode = 500 };
            }
        }
    }
}
