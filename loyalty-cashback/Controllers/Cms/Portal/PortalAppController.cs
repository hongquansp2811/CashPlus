using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;

namespace LOYALTY.Controllers
{
    [Route("api/app/portalApp")]
    //[Authorize(Policy = "AppUser")]
    [ApiController]
    public class PortalAppController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOtherListType _otherListType;
        private readonly ILoggingHelpers _logging;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ICommonFunction _commonFunction;
        public PortalAppController(IConfiguration configuration, IOtherListType otherListType, ILoggingHelpers logging, LOYALTYContext context, IEmailSender emailSender, ICommonFunction commonFunction)
        {
            _configuration = configuration;
            this._otherListType = otherListType;
            this._logging = logging;
            this._context = context;
            _emailSender = emailSender;
            _commonFunction = commonFunction;
        }

        [Route("nation")]
        [HttpGet]
        public JsonResult GetListNation()
        {
            object data = (from p in _context.Provinces
                           where p.types == 0
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("province")]
        [HttpGet]
        public JsonResult GetListProvince()
        {
            object data = (from p in _context.Provinces
                           where p.parent_id == 20000
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("provinceBy/{id}")]
        [HttpGet]
        public JsonResult GetListProvinceBy(int id)
        {
            object data = (from p in _context.Provinces
                           where p.parent_id == id
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("otherListByCode/{code}")]
        [HttpGet]
        public JsonResult GetListOtherListByCode(string code)
        {
            object data = (from p in _context.OtherLists
                           join m in _context.OtherListTypes on p.type equals m.id into ms
                           from m in ms.DefaultIfEmpty()
                           where p.status == 1 && m.code == code
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("servicetype")]
        [HttpGet]
        public JsonResult GetListServiceType()
        {
            object data = (from p in _context.ServiceTypes
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name,
                               discount_rate = p.discount_rate
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("productlabel")]
        [HttpGet]
        public JsonResult GetListProductLabel()
        {
            object data = (from p in _context.ProductLabels
                           where p.status == 1
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("registerStore")]
        [HttpPost]
        public JsonResult RegisterStore(PartnerRequest request)
        {
            if (request.name == null)
            {
                return new JsonResult(new APIResponse("ERROR_NAME_MISSING")) { StatusCode = 200 };
            }

            if (request.store_type_id == null)
            {
                return new JsonResult(new APIResponse("ERROR_STORE_TYPE_ID_MISSING")) { StatusCode = 200 };
            }

            if (request.store_owner == null)
            {
                return new JsonResult(new APIResponse("ERROR_STORE_OWNER_MISSING")) { StatusCode = 200 };
            }

            if (request.phone == null)
            {
                return new JsonResult(new APIResponse("ERROR_PHONE_MISSING")) { StatusCode = 200 };
            }

            if (request.email == null)
            {
                return new JsonResult(new APIResponse("ERROR_EMAIL_MISSING")) { StatusCode = 200 };
            }

            if (request.address == null)
            {
                return new JsonResult(new APIResponse("ERROR_ADDRESS_MISSING")) { StatusCode = 200 };
            }

            Random random = new Random();
            const string chars = "123456789";
            var login_code = new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());

            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Tạo cửa hàng
                var data = new Partner();
                data.id = Guid.NewGuid();
                data.code = "REGISTER";
                data.name = request.name;
                data.store_type_id = request.store_type_id;
                data.phone = request.phone;
                data.email = request.email;
                data.store_owner = request.store_owner;
                data.province_id = request.province_id;
                data.district_id = request.district_id;
                data.ward_id = request.ward_id;
                data.address = request.address;
                data.status = 14; // Check Trạng thái
                data.total_rating = 0;
                data.rating = 0;
                data.discount_rate = request.discount_rate;
                data.support_person_id = request.support_person_id;
                data.support_person_phone = request.support_person_phone;
                data.is_delete = false;
                data.login_code = login_code;
                data.is_confirm_email_register = false;

                data.user_created = request.name;
                data.user_updated = request.name;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.Partners.Add(data);
                _context.SaveChanges();

                // Gửi email Cho đối tác xác nhận link đăng ký
                try
                {
                    string message = "<p>Xin chào " + request.name + "!<p>";
                    message += "<br/>";
                    message += "<p>Cảm ơn Anh/Chị đã đăng ký trở thành Đối tác của CashPlus.</p>";
                    message += "<p>Để tiếp tục đăng ký, Anh/Chị vui lòng nhấp vào <a href='" + Consts.PORTAL_URL + "/xac-nhan-dang-ky-doi-tac?login_code=" + login_code + "'>đường dẫn</a> để cung cấp đầy đủ thông tin để chúng tôi hoàn thiện hồ sơ của anh chị sớm nhất!</p>";
                    message += "<br/>";
                    message += "<p>Trân trọng!</p>";
                    message += "<br/>";
                    message += "<p>@2023 ATS Group</p>";
                    _emailSender.SendEmailAsync(request.email, "[CashPlus] - Xác nhận đăng ký đối tác", message);
                }
                catch (Exception ex2)
                {

                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new APIResponse("ERROR_ADD_FAIL")) { StatusCode = 200 };
            }

            transaction.Commit();
            transaction.Dispose();
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }

        [Route("getStoreInfoByCode")]
        [HttpPost]
        public JsonResult GetStoreInfoByCode(PartnerRequest request)
        {
            if (request.login_code == null || request.login_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_LOGIN_CODE_NOT_FOUND")) { StatusCode = 200 };
            }

            var data = _context.Partners.Where(x => x.login_code == request.login_code).FirstOrDefault();

            if (data == null)
            {
                return new JsonResult(new APIResponse("ERROR_LOGIN_CODE_INCORRECT")) { StatusCode = 200 };
            }

            if (data.is_confirm_email_register == true)
            {
                return new JsonResult(new APIResponse("ERROR_LOGIN_CODE_IS_CONFIRM")) { StatusCode = 200 };
            }

            // Nhập email gắn nhân viên kinh doanh
            if (request.support_person_email != null && request.support_person_email.Length > 0)
            {
                var userObj = _context.Users.Where(x => x.is_admin == true && x.email == request.support_person_email).FirstOrDefault();

                if (userObj != null)
                {
                    data.support_person_id = userObj.id;
                    data.support_person_phone = userObj.phone;

                    _context.SaveChanges();
                }
            }

            var dataResult = (from p in _context.Partners
                              join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                              from sv in svs.DefaultIfEmpty()
                              join st in _context.OtherLists on p.status equals st.id into sts
                              from st in sts.DefaultIfEmpty()
                              where p.id == data.id
                              select new
                              {
                                  id = p.id,
                                  service_type_id = p.service_type_id,
                                  service_type_name = sv != null ? sv.name : "",
                                  store_type_id = p.store_type_id,
                                  code = p.code,
                                  name = p.name,
                                  phone = p.phone,
                                  email = p.email,
                                  store_owner = p.store_owner,
                                  address = p.address,
                                  start_hour = p.start_hour,
                                  end_hour = p.end_hour,
                                  working_day = p.working_day,
                                  tax_tncn = p.tax_tncn,
                                  tax_code = p.tax_code,
                                  description = p.description,
                                  product_label_id = p.product_label_id,
                                  avatar = p.avatar,
                                  status = p.status,
                                  status_name = st != null ? st.name : "",
                                  province_id = p.province_id,
                                  district_id = p.district_id,
                                  ward_id = p.ward_id,
                                  latitude = p.latitude,
                                  longtitude = p.longtitude,
                                  license_image = p.license_image,
                                  license_no = p.license_no,
                                  license_person_number = p.license_person_number,
                                  license_owner = p.license_owner,
                                  license_date = p.license_date != null ? _commonFunction.convertDateToStringSort(p.license_date) : "",
                                  license_birth_date = p.license_birth_date != null ? _commonFunction.convertDateToStringSort(p.license_birth_date) : "",
                                  license_nation_id = p.license_nation_id,
                                  indetifier_no = p.indetifier_no,
                                  identifier_date = p.identifier_date != null ? _commonFunction.convertDateToStringSort(p.identifier_date) : "",
                                  identifier_at = p.identifier_at,
                                  identifier_date_expire = p.identifier_date_expire != null ? _commonFunction.convertDateToStringSort(p.identifier_date_expire) : "",
                                  identifier_address = p.identifier_address,
                                  identifier_nation_id = p.identifier_nation_id,
                                  identifier_province_id = p.identifier_province_id,
                                  is_same_address = p.is_same_address,
                                  now_address = p.now_address,
                                  now_nation_id = p.now_nation_id,
                                  now_province_id = p.now_province_id,
                                  identifier_front_image = p.identifier_front_image,
                                  identifier_back_image = p.identifier_back_image,
                                  discount_rate = p.discount_rate,
                                  support_person_id = p.support_person_id,
                                  support_person_phone = p.support_person_phone,
                                  list_bank_accounts = _context.CustomerBankAccounts.Where(x => x.user_id == p.id).ToList(),
                                  list_documents = _context.PartnerDocuments.Where(x => x.partner_id == p.id).ToList()
                              }).FirstOrDefault();
            return new JsonResult(new APIResponse(dataResult))
            { StatusCode = 200 };
        }

        [Route("updateInfoStore")]
        [HttpPost]
        public JsonResult UpdateInfoStore(PartnerRequest request)
        {
            if (request.login_code == null || request.login_code.Length == 0)
            {
                return new JsonResult(new APIResponse("ERROR_LOGIN_CODE_NOT_FOUND")) { StatusCode = 200 };
            }

            var data = _context.Partners.Where(x => x.login_code == request.login_code).FirstOrDefault();

            if (data == null)
            {
                return new JsonResult(new APIResponse("ERROR_LOGIN_CODE_INCORRECT")) { StatusCode = 200 };
            }

            if (data.is_confirm_email_register == true)
            {
                return new JsonResult(new APIResponse("ERROR_LOGIN_CODE_IS_CONFIRM")) { StatusCode = 200 };
            }

            var transaction = _context.Database.BeginTransaction();

            try
            {
                var serviceTypeObj = _context.ServiceTypes.Where(x => x.id == request.service_type_id).FirstOrDefault();

                string serviceTypeCode = (serviceTypeObj != null && serviceTypeObj.code != null) ? serviceTypeObj.code : "LDV";
                var maxCodeObject = _context.Partners.Where(x => x.code != null && x.code.Contains(serviceTypeCode)).OrderByDescending(x => x.code).FirstOrDefault();
                string code = "";
                if (maxCodeObject == null)
                {
                    code = serviceTypeCode + "00000001";
                }
                else
                {
                    string maxCode = maxCodeObject.code;
                    maxCode = maxCode.Substring(serviceTypeCode.Length);
                    int orders = int.Parse(maxCode);
                    orders = orders + 1;
                    string orderString = orders.ToString();
                    char pad = '0';
                    int number = 8;
                    code = serviceTypeCode + orderString.PadLeft(number, pad);
                }

                var userObj = _context.Partners.Where(x => x.username == request.username).FirstOrDefault();
                if (userObj != null)
                {
                    return new JsonResult(new APIResponse("Tên tài khoản đã tồn tại trên hệ thống!!")) { StatusCode = 500 };
                }

                data.code = code;
                data.name = request.name;
                data.service_type_id = request.service_type_id;
                data.store_type_id = request.store_type_id;
                data.store_owner = request.store_owner;
                data.avatar = request.avatar;
                data.phone = request.phone;
                data.email = request.email;
                data.start_hour = request.start_hour;
                data.end_hour = request.end_hour;
                data.working_day = request.working_day;
                data.tax_tncn = request.tax_tncn;
                data.tax_code = request.tax_code;
                data.description = request.description;
                data.product_label_id = request.product_label_id;
                data.province_id = request.province_id;
                data.district_id = request.district_id;
                data.ward_id = request.ward_id;
                data.address = request.address;
                data.latitude = request.latitude;
                data.longtitude = request.longtitude;
                data.support_person_id = request.support_person_id;
                data.support_person_phone = request.support_person_phone;
                data.discount_rate = request.discount_rate;


                // Bổ sung 14/09
                data.license_image = request.license_image;
                data.license_no = request.license_no;
                data.license_person_number = request.license_person_number;
                data.license_owner = request.license_owner;
                data.support_person_id = request.support_person_id;
                if (request.license_date != null)
                {
                    data.license_date = _commonFunction.convertStringSortToDate(request.license_date);
                }

                if (request.license_birth_date != null)
                {
                    data.license_birth_date = _commonFunction.convertStringSortToDate(request.license_birth_date);
                }

                if (request.identifier_date != null)
                {
                    data.identifier_date = _commonFunction.convertStringSortToDate(request.identifier_date);
                }

                if (request.identifier_date_expire != null)
                {
                    data.identifier_date_expire = _commonFunction.convertStringSortToDate(request.identifier_date_expire);
                }
                data.license_nation_id = request.license_nation_id;
                data.indetifier_no = request.indetifier_no;
                data.identifier_at = request.identifier_at;
                data.identifier_address = request.identifier_address;
                data.identifier_province_id = request.identifier_province_id;
                data.is_same_address = request.is_same_address != null ? request.is_same_address : false;
                data.now_address = request.now_address;
                data.now_nation_id = request.now_nation_id;
                data.now_province_id = request.now_province_id;
                data.identifier_front_image = request.identifier_front_image;
                data.identifier_back_image = request.identifier_back_image;
                data.owner_percent = request.owner_percent;
                data.identifier_nation_id = request.identifier_nation_id;
                data.is_confirm_email_register = true;

                _context.SaveChanges();

                var lstDeletes = _context.PartnerDocuments.Where(x => x.partner_id == data.id).ToList();
                _context.PartnerDocuments.RemoveRange(lstDeletes);

                _context.SaveChanges();

                // Tạo tài liệu
                if (request.list_documents != null && request.list_documents.Count > 0)
                {
                    for (int i = 0; i < request.list_documents.Count; i++)
                    {
                        var item = new PartnerDocument();
                        item.id = Guid.NewGuid();
                        item.partner_id = data.id;
                        item.file_name = request.list_documents[i].file_name;
                        item.links = request.list_documents[i].links;
                        _context.PartnerDocuments.Add(item);
                    }

                    _context.SaveChanges();
                }

                if (request.username != null && request.username.Length > 0)
                {
                    // Tạo share code
                    Random random = new Random();
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var stringReturn = new string(Enumerable.Repeat(chars, 9).Select(s => s[random.Next(s.Length)]).ToArray());

                    // Tạo User cửa hàng admin
                    var newUser = new User();
                    newUser.id = Guid.NewGuid();
                    newUser.code = request.code;
                    newUser.full_name = request.name;
                    newUser.avatar = request.avatar;
                    newUser.email = request.email;
                    newUser.phone = request.phone;
                    newUser.username = request.username;
                    newUser.password = _commonFunction.ComputeSha256Hash(request.password);
                    newUser.status = 1;
                    newUser.is_sysadmin = false;
                    newUser.is_admin = false;
                    newUser.is_customer = false;
                    newUser.is_partner_admin = true;
                    newUser.is_partner = true;
                    newUser.partner_id = data.id;
                    newUser.total_point = 0;
                    newUser.point_waiting = 0;
                    newUser.point_avaiable = 0;
                    newUser.point_affiliate = 0;
                    newUser.share_code = stringReturn;
                    newUser.is_delete = false;
                    newUser.user_created = request.name;
                    newUser.user_updated = request.name;
                    newUser.date_created = DateTime.Now;
                    newUser.date_updated = DateTime.Now;
                    _context.Users.Add(newUser);
                    // Save Changes
                    _context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            transaction.Commit();
            transaction.Dispose();
            return new JsonResult(new APIResponse(200)) { StatusCode = 200 };
        }
    }
}
