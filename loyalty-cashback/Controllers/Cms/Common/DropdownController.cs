using LOYALTY.Data;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace LOYALTY.Controllers
{
    [Route("api/dropdown")]
    [ApiController]
    public class DropdownController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IOtherListType _otherListType;
        private readonly ILoggingHelpers _logging;
        private readonly LOYALTYContext _context;
        private static string cms_masterdata_province = "cms_masterdata_province";
        private static string cms_masterdata_bank = "cms_masterdata_bank";
        private static string cms_authen_function = "cms_authen_function";
        private static string cms_authen_user = "cms_authen_user";
        private static string cms_masterdata_bloglist_category = "cms_masterdata_bloglist_category";
        private static string cms_masterdata_notificationtype = "cms_masterdata_notificationtype";
        private static string cms_masterdata_productgroup = "cms_masterdata_productgroup";
        private static string cms_masterdata_servicetype = "cms_masterdata_servicetype";
        private static string cms_masterdata_productlabel = "cms_masterdata_productlabel";
        private static string cms_partner = "cms_partner";
        private static string cms_authen_customer = "cms_authen_customer";
        private static string cms_masterdata_customerrank = "cms_masterdata_customerrank";
        private static string cms_business_contract = "cms_business_contract";
        private static string cms_masterdata_complaininfo = "cms_masterdata_complaininfo";

        public DropdownController(IConfiguration configuration, IOtherListType otherListType, ILoggingHelpers logging, LOYALTYContext context)
        {
            _configuration = configuration;
            this._otherListType = otherListType;
            this._logging = logging;
            this._context = context;
        }

        [Route("usergroup")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListUserGroup()
        {
            object data = (from p in _context.UserGroups
                           where p.status == 1
                           select new
                           {
                               id = p.id,
                               name = p.name,
                               description = p.description
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("nation")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListNation()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_province, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
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
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_province, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
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
        [Authorize]
        [HttpGet]
        public JsonResult GetListProvinceBy(int id)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_province, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.Provinces
                           where p.parent_id == id
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }


        [Route("bank")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListBank()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_bank, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.Banks
                           select new
                           {
                               id = p.id,
                               avatar = p.avatar,
                               name = p.name,
                               description = p.description
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("bankActive")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListBankActive()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_bank, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.Banks
                           where p.active == true
                           select new
                           {
                               id = p.id,
                               avatar = p.avatar,
                               name = p.name,
                               description = p.description
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("status")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListStatus()
        {
            object data = (from p in _context.OtherLists
                           join m in _context.OtherListTypes on p.type equals m.id into ms
                           from m in ms.DefaultIfEmpty()
                           where p.status == 1 && m.code == "STATUS"
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("otherListByCode/{code}")]
        [Authorize]
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
                               code = p.code,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Authorize]
        [HttpGet("function")]
        public JsonResult GetListFunction()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_authen_function, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.Functions
                           where p.status == 1
                           select new
                           {
                               id = p.id,
                               name = p.name,
                               function_code = p.code
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("userApprove")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListUserApprove()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_authen_user, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.Users
                           where p.status == 1 && p.is_admin == true
                           select new
                           {
                               id = p.id,
                               name = p.full_name,
                               phone = p.phone,
                               user_group_id = p.user_group_id
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("blogCategory")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListBlogCategory()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_bloglist_category, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.BlogCategorys
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("notificationType")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListNotificationType()
        {

            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_notificationtype, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.NotificationTypes
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("otherlistType")]
        [Authorize]
        [HttpGet]
        public JsonResult otherlistType()
        {
            object data = (from p in _context.OtherListTypes
                           where p.status == 1
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("productgroup")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListProductGroup()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_productgroup, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.ProductGroups
                           where p.status == 1
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("servicetype")]
        [HttpGet]
        public JsonResult GetListServiceType()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_servicetype, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.ServiceTypes
                           orderby p.orders ascending
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name,
                               icons = p.icons,
                               orders = p.orders,
                               discount_rate = p.discount_rate
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("productlabel")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListProductLabel()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_productlabel, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.ProductLabels
                           where p.status == 1
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name,
                               order = p.orders
                           }).OrderBy(p => p.order).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("partner")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListPartner()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_partner, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.Partners
                           join st in _context.ServiceTypes on p.service_type_id equals st.id
                           where p.status == 15 && (p.is_delete == null || p.is_delete != true)
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               name = p.name,
                               store_owner = p.store_owner,
                               phone = p.phone,
                               service_type_id = p.service_type_id,
                               service_type_name = st.name,
                               discount_rate = p.discount_rate
                           }).Take(1000).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("customer")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListCustomer()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_authen_customer, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.Customers
                           where p.status == 1
                           select new
                           {
                               id = p.id,
                               full_name = p.full_name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("customerrank")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListCustomerRank()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_customerrank, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.CustomerRanks
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("partnercontract")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListPartnerContract()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_business_contract, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.PartnerContracts
                           join s in _context.Partners on p.partner_id equals s.id
                           join st in _context.ServiceTypes on p.service_type_id equals st.id
                           where p.status == 12
                           select new
                           {
                               id = p.id,
                               contract_no = p.contract_no,
                               partner_id = p.partner_id,
                               partner_name = s.name,
                               service_type_id = p.service_type_id,
                               service_type_name = st.name,
                               discount_rate = p.discount_rate,
                               partner_code = s.code
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("userAdmin")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListUserAdmin()
        {
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
            var full_name =  username.Value;
            var is_admin = _context.Users.Where(p => p.username == username.Value).Select(p => p.is_admin).FirstOrDefault();
            if(is_admin == false)
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }

            object data = (from p in _context.Users
                           where p.status == 1 && p.is_admin == true
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               full_name = p.full_name,
                               phone = p.phone
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }
        [Route("userAdminTest")]
        [HttpGet]
        public JsonResult GetListUserAdminTest()
        {
            object data = (from p in _context.Users
                           where p.status == 1 && p.is_admin == true
                           join t in _context.UserGroups on p.user_group_id equals t.id into ts
                           from t in ts.DefaultIfEmpty()
                           where t.code == "USER_SALE"
                           select new
                           {
                               id = p.id,
                               code = p.code,
                               full_name = p.full_name,
                               phone = p.phone
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }
        [Route("complainInfo")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListComplainInfo()
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_masterdata_complaininfo, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            object data = (from p in _context.ComplainInfos
                           select new
                           {
                               id = p.id,
                               name = p.name
                           }).ToList();
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        public class DropdownResponse
        {
            public string? id { get; set; }
            public string? name { get; set; }
        }

        [Route("paymentType")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListPaymentType()
        {
            List<DropdownResponse> data = new List<DropdownResponse>();
            //data.Add(new DropdownResponse
            //{
            //    id = "Cash",
            //    name = "Tiền mặt"
            //});

            //data.Add(new DropdownResponse
            //{
            //    id = "BaoKim",
            //    name = "Thanh toán online"
            //});

            data.Add(new DropdownResponse
            {
                id = "Cash",
                name = "Hoàn tiền"
            });

            data.Add(new DropdownResponse
            {
                id = "Point",
                name = "Tích điểm"
            });
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }

        [Route("paymentType2")]
        [Authorize]
        [HttpGet]
        public JsonResult GetListPaymentType2()
        {
            List<DropdownResponse> data = new List<DropdownResponse>();
            data.Add(new DropdownResponse
            {
                id = "Cash",
                name = "Tiền mặt"
            });

            data.Add(new DropdownResponse
            {
                id = "BaoKim",
                name = "Thanh toán online"
            });

            //data.Add(new DropdownResponse
            //{
            //    id = "Cash",
            //    name = "Hoàn tiền"
            //});

            //data.Add(new DropdownResponse
            //{
            //    id = "Point",
            //    name = "Tích điểm"
            //});
            return new JsonResult(new APIResponse(data)) { StatusCode = 200 };
        }
    }
}
