using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;

namespace LOYALTY.Controllers
{
    [Route("api/export")]
    [Authorize]
    [ApiController]
    public class ExportController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ILoggingHelpers _logging;
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;

        private static string cms_config_bonusconfig = "cms_config_bonusconfig";
        private static string cms_business_contract = "cms_business_contract";
        private static string cms_business_changepointorder = "cms_business_changepointorder";
        private static string cms_authen_customer = "cms_authen_customer";
        private static string cms_config_affiliateconfig = "cms_config_affiliateconfig";
        private static string cms_authen_user = "cms_authen_user";
        private static string cms_information_reporting_partner = "cms_partner";
        private static string cms_business_recallpointorder = "cms_business_recallpointorder";
        private static string cms_business_product = "cms_business_product";
        private static string cms_business_rating = "cms_business_rating";

        private static string cms_business_complain = "cms_business_complain";
        public ExportController(IConfiguration configuration, ILoggingHelpers logging, LOYALTYContext context, ICommonFunction commonFunction)
        {
            _configuration = configuration;
            this._logging = logging;
            this._context = context;
            this._commonFunction = commonFunction;
        }

        // Phiếu đổi điểm
        [Route("changePoint")]
        [HttpPost]
        public ActionResult ExportChangePointOrder(ChangePointOrderRequest request)
        {

            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_business_changepointorder, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }

            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;

            var lstData = (from p in _context.ChangePointOrders
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           join c in _context.Customers on p.user_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on p.user_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               user_type = c != null ? "CUSTOMER" : "PARTNER",
                               user_type_name = c != null ? "Khách hàng" : "Đối tác",
                               username = c != null ? c.phone : s.username,
                               trans_no = p.trans_no,
                               trans_date = p.date_created,
                               trans_date_2 = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               point_exchange = p.point_exchange,
                               value_exchange = p.value_exchange,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()) || x.username.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.user_type != null)
            {
                lstData = lstData.Where(x => x.user_type == request.user_type);
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            if (request.from_point != null)
            {
                lstData = lstData.Where(x => x.point_exchange >= request.from_point);
            }

            if (request.to_point != null)
            {
                lstData = lstData.Where(x => x.point_exchange <= request.to_point);
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();

            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản/SĐT</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số điểm quy đổi</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số tiền nhận được (VNĐ)</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày tạo phiếu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].user_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].username + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].point_exchange) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].value_exchange) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_date_2 + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Khách hàng
        [Route("customer")]
        [HttpPost]
        public ActionResult ExportCustomer(CustomerRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_authen_customer, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.Customers
                           join u in _context.Users on p.id equals u.customer_id
                           join sp in _context.Users on u.share_person_id equals sp.id into sps
                           from sp in sps.DefaultIfEmpty()
                           join r in _context.CustomerRanks on p.customer_rank_id equals r.id into rs
                           from r in rs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               phone = p.phone,
                               full_name = p.full_name,
                               share_person = sp != null ? sp.username : "",
                               birth_date = p.birth_date != null ? _commonFunction.convertDateToStringSort(p.birth_date) : "",
                               customer_rank_id = p.customer_rank_id,
                               customer_rank_name = r != null ? r.name : "",
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.phone != null && request.phone.Length > 0)
            {
                lstData = lstData.Where(x => x.phone.Trim().ToLower().Contains(request.phone.Trim().ToLower()) || x.full_name.Trim().ToLower().Contains(request.phone.Trim().ToLower()));
            }

            if (request.customer_rank_id != null)
            {
                lstData = lstData.Where(x => x.customer_rank_id == request.customer_rank_id);
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();

            var lst = lstData.ToList();


            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Họ và tên</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Người giới thiệu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày sinh</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Hạng khách hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].phone + "</td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].full_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].share_person + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].birth_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].customer_rank_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Cấu hình tích điểm
        [Route("accumulatepointconfig")]
        [HttpPost]
        public ActionResult ExportAccumulatePointConfig(AccumulatePointConfigRequest request)
        {
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.AccumulatePointConfigs
                           join c in _context.PartnerContracts on p.contract_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on p.partner_id equals s.id
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id
                           where p.code == null
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               from_date_origin = p.from_date,
                               to_date_origin = p.to_date,
                               from_date = _commonFunction.convertDateToStringSort(p.from_date),
                               to_date = _commonFunction.convertDateToStringSort(p.to_date),
                               contract_id = p.contract_id,
                               contract_no = c.contract_no,
                               partner_code = s.code,
                               partner_name = s.name,
                               active = p.active,
                               service_type_id = p.service_type_id,
                               service_type_name = sv.name,
                               discount_rate = p.discount_rate,
                               description = p.description
                           });

            // Nếu tồn tại Where theo tên
            if (request.search != null && request.search.Length > 0)
            {
                lstData = lstData.Where(x => x.partner_code.Trim().ToLower().Contains(request.search.Trim().ToLower()) || x.partner_name.Trim().ToLower().Contains(request.search.Trim().ToLower()) || x.contract_no.Trim().ToLower().Contains(request.search.Trim().ToLower()));
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số hợp đồng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại dịch vụ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Diễn giải</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>% chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].contract_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].service_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].description + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].discount_rate + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + (lst[i].active == true ? "Hiệu lực" : "Chưa áp dụng") + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table>");
            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".xls");
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-excel");
        }

        // Cấu hình Affiliate
        [Route("affiliatepointconfig")]
        [HttpPost]
        public ActionResult ExportAffiliateConfig(AffiliateConfigRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_config_affiliateconfig, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.AffiliateConfigs
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           where p.code == null
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               from_date_origin = p.from_date,
                               to_date_origin = p.to_date,
                               from_date = _commonFunction.convertDateToStringSort(p.from_date),
                               to_date = _commonFunction.convertDateToStringSort(p.to_date),
                               active = p.active,
                               service_type_id = p.service_type_id,
                               service_type_name = sv.name,
                               description = p.description
                           });

            // Nếu tồn tại Where theo tên
            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
            }

            if (request.active != null)
            {
                lstData = lstData.Where(x => x.active == request.active);
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày áp dụng từ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày áp dụng tới</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại dịch vụ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Diễn giải</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].from_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].to_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].service_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].description + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + (lst[i].active == true ? "Hiệu lực" : "Chưa áp dụng") + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Cấu hình thưởng nạp điểm
        [Route("bonuspointconfig")]
        [HttpPost]
        public ActionResult ExportBonusPointConfig(AffiliateConfigRequest request)
        {
            // Default page_no, page_size

            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_config_bonusconfig, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.BonusPointConfigs
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               from_date_origin = p.from_date,
                               to_date_origin = p.to_date,
                               from_date = _commonFunction.convertDateToStringSort(p.from_date),
                               to_date = _commonFunction.convertDateToStringSort(p.to_date),
                               active = p.active,
                               service_type_id = p.service_type_id,
                               service_type_name = sv.name,
                               description = p.description,
                               min_point = p.min_point,
                               max_point = p.max_point,
                               discount_rate = p.discount_rate
                           });

            // Nếu tồn tại Where theo tên
            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
            }

            if (request.active != null)
            {
                lstData = lstData.Where(x => x.active == request.active);
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày áp dụng từ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày áp dụng tới</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại dịch vụ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>% Thưởng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điểm tối thiểu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điểm tối đa</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Diễn giải</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].from_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].to_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].service_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].discount_rate + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].min_point + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].max_point + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].description + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + (lst[i].active == true ? "Hiệu lực" : "Chưa áp dụng") + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Phiếu nạp điểm
        [Route("addpointorder")]
        [HttpPost]
        public ActionResult ExportAddPointOrder(AddPointOrderRequest request)
        {
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.AddPointOrders
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join cf in _context.CustomerFakeBanks on p.partner_id equals cf.user_id into cfs
                           from cf in cfs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                               date_created = p.date_created,
                               partner_id = p.partner_id,
                               partner_code = s.code,
                               partner_name = s.name,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_exchange,
                               bank_name = cf != null ? cf.bank_name : "",
                               bank_no = cf != null ? cf.bank_account : "",
                               status_name = "Hoàn thành"
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.partner_id == request.partner_id);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tên cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tổng tiền thanh toán</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tổng điểm nạp</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngân hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].bill_amount + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].point_exchange + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].bank_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].bank_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + "Hoàn thành" + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Phiếu tích điểm
        [Route("accumulatepointorder")]
        [HttpPost]
        public ActionResult ExportAccumulatePointOrder(AccumulatePointOrderRequest request)
        {
            // Default page_no, page_size
            //if (request.page_size < 1)
            //{
            //    request.page_size = Consts.PAGE_SIZE;
            //}

            //if (request.page_no < 1)
            //{
            //    request.page_no = 1;
            //}
            //// Số lượng Skip
            //int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu


            var lstData = (from p in _context.AccumulatePointOrders
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                               date_created = p.date_created,
                               customer_phone = c != null ? c.phone : "",
                               partner_code = s.code,
                               partner_name = s.name,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_exchange,
                               point_customer = p.point_customer,
                               point_system = p.point_system,
                               point_partner = p.point_partner,
                               discount_rate = p.discount_rate,
                               description = p.description,
                               status = p.status,
                               status_name = st != null ? st.name : "",
                               return_type = p.return_type,
                               payment_type = p.payment_type,
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            //// Đếm số lượng
            //int countElements = lstData.Count();

            //// Số lượng trang
            //int totalPage = countElements > 0
            //        ? (int)Math.Ceiling(countElements / (double)request.page_size)
            //        : 0;

            //var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản khách hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã đối tác</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tổng hóa đơn</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>% chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Hình thứ thanh toán</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_date + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].customer_phone + "</td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].bill_amount) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].discount_rate + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].point_partner) + "</font></td>");
                var payment = lst[i].payment_type == "BaoKim" ? "Online" : "Tiền mặt";
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + payment + "</font></td>");
                string return_type = lst[i].return_type == "Point" ? "Tích điểm" : "Hoàn tiền";
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + return_type + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Báo cáo doanh thu
        [Route("revenuereport")]
        [HttpPost]
        public ActionResult ExportRevenueReport(AccumulatePointOrderRequest request)
        {
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var fromDate = request.from_date != null && request.from_date.Length == 10 ? _commonFunction.convertStringSortToDate(request.from_date).Date : DateTime.Now.AddYears(-10);
            var toDate = request.to_date != null && request.to_date.Length == 10 ? _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1) : DateTime.Now;

            var lstData = (from p in _context.Users
                           where (p.is_customer == true) || (p.is_partner_admin == true)
                           select new
                           {
                               user_type = p.is_customer == true ? "CUSTOMER" : "PARTNER",
                               user_type_name = p.is_customer == true ? "Khách hàng" : "Cửa hàng (Đối tác)",
                               username = p.username,
                               accumulate_point = p.is_customer == true ? _context.AccumulatePointOrders.Where(x => x.customer_id == p.customer_id && x.status == 5 && x.date_created >= fromDate && x.date_created <= toDate).Sum(x => x.point_customer) : 0,
                               affiliate_point = p.is_customer == true ? _context.CustomerPointHistorys.Where(x => x.customer_id == p.customer_id && x.order_type.Contains("AFF_") && x.status != 6 && x.trans_date >= fromDate && x.trans_date <= toDate).Sum(x => x.point_amount) : _context.PartnerPointHistorys.Where(x => x.partner_id == p.partner_id && x.order_type.Contains("AFF_") && x.status != 6 && x.trans_date >= fromDate && x.trans_date <= toDate).Sum(x => x.point_amount),
                               point_avaiable = p.point_avaiable,
                               point_waiting = p.point_waiting + p.point_affiliate,
                               total_point = p.total_point
                           });

            if (request.user_type != null)
            {
                lstData = lstData.Where(x => x.user_type == request.user_type);
            }

            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.username.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tích điểm giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tích điểm đội nhóm</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điểm khả dụng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điểm chờ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tổng điểm</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].user_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].username + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].accumulate_point) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].affiliate_point) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].point_avaiable) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].point_waiting) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].total_point) + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Người dùng
        [Route("user")]
        [HttpPost]
        public ActionResult ExportUser(UserRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_authen_user, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            var lstData = (from p in _context.Users
                           join f in _context.UserGroups on p.user_group_id equals f.id into fs
                           from f in fs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.is_admin == true && p.is_delete != true
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               username = p.username,
                               code = p.code,
                               full_name = p.full_name,
                               phone = p.phone,
                               email = p.email,
                               user_group_id = p.user_group_id,
                               user_group_name = f != null ? f.name : "",
                               status = p.status,
                               status_name = st != null ? st.name : "",
                               date_updated = _commonFunction.convertDateToStringSort(p.date_updated)
                           });
            // Nếu tồn tại Where theo tên
            if (request.full_name != null && request.full_name.Length > 0)
            {
                lstData = lstData.Where(x => x.full_name.Trim().ToLower().Contains(request.full_name.Trim().ToLower())
                || x.email.Trim().ToLower().Contains(request.full_name.Trim().ToLower()) || x.code.Trim().ToLower().Contains(request.full_name.Trim().ToLower()));
            }

            if (request.user_group_id != null)
            {
                lstData = lstData.Where(x => x.user_group_id == request.user_group_id);
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Họ và tên</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Nhóm quyền</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số điện thoại</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày cập nhật</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].username + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].full_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].user_group_name + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].phone + "</td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].date_updated + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Đối tác
        [Route("partner")]
        [HttpPost]
        public ActionResult ExportPartner(PartnerRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_information_reporting_partner, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            var lstData = (from p in _context.Partners
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.is_delete != true
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               service_type_id = p.service_type_id,
                               service_type_name = sv != null ? sv.name : "",
                               code = p.code,
                               name = p.name,
                               phone = p.phone,
                               store_owner = p.store_owner,
                               address = p.address,
                               status = p.status,
                               status_name = st != null ? st.name : "",
                               province_id = p.province_id,
                               district_id = p.district_id,
                               ward_id = p.ward_id,
                               id_contract = _context.PartnerContracts.Where(l => l.partner_id == p.id && l.status == 12).Select(l => l.id).FirstOrDefault()
                           });
            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstData = lstData.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.phone.Trim().ToLower().Contains(request.name.Trim().ToLower())
                || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.province_id != null)
            {
                lstData = lstData.Where(x => x.province_id == request.province_id);
            }

            if (request.district_id != null)
            {
                lstData = lstData.Where(x => x.district_id == request.district_id);
            }

            if (request.ward_id != null)
            {
                lstData = lstData.Where(x => x.ward_id == request.ward_id);
            }

            //lstData = lstData.Where(l => l.is_delete != true);

            var lst = lstData.ToList();

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại dịch vụ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tên cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điện thoại</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tên chủ cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Địa chỉ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].service_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].name + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].phone + "</td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].store_owner + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].address + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Hợp đồng
        [Route("contract")]
        [HttpPost]
        public ActionResult ExportContract(PartnerContractRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_business_contract, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            var lstData = (from p in _context.PartnerContracts
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           join sto in _context.Partners on p.partner_id equals sto.id into stos
                           from sto in stos.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               service_type_id = p.service_type_id,
                               service_type_name = sv != null ? sv.name : "",
                               from_date_origin = p.from_date,
                               to_date_origin = p.to_date,
                               from_date = _commonFunction.convertDateToStringSort(p.from_date),
                               to_date = _commonFunction.convertDateToStringSort(p.to_date),
                               partner_code = sto.code,
                               partner_name = sto.name,
                               discount_rate = p.discount_rate,
                               is_delete = p.is_delete,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.contract_name != null && request.contract_name.Length > 0)
            {
                lstData = lstData.Where(x => x.partner_code.Trim().ToLower().Contains(request.contract_name.Trim().ToLower()) || x.partner_name.Trim().ToLower().Contains(request.contract_name.Trim().ToLower()));
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.from_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }
            lstData = lstData.Where(l => l.is_delete != true);

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày áp dụng từ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày áp dụng đến</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại dịch vụ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tên cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>% chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái hợp đồng</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].from_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].to_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].service_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].discount_rate + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Đơn hàng
        [Route("order")]
        [HttpPost]
        public ActionResult ExportOrder(PartnerOrderRequest request)
        {
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            var lstData = (from p in _context.PartnerOrders
                           join c in _context.Customers on p.customer_id equals c.id
                           join s in _context.Partners on p.partner_id equals s.id
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.order_date descending
                           select new
                           {
                               id = p.id,
                               order_code = p.order_code,
                               order_date = p.order_date,
                               order_date_2 = _commonFunction.convertDateToStringFull(p.order_date),
                               customer_name = c.full_name,
                               customer_phone = c.phone,
                               partner_id = p.partner_id,
                               partner_name = s.name,
                               status = p.status,
                               status_name = st != null ? st.name : "",
                               total_amount = p.total_amount,
                               total_quantity = _context.PartnerOrderDetails.Where(x => x.partner_order_id == p.id).Sum(x => x.quantity),
                               list_items = (from d in _context.PartnerOrderDetails
                                             join pr in _context.Products on d.product_id equals pr.id
                                             where d.partner_order_id == p.id
                                             select new
                                             {
                                                 product_name = pr.name,
                                                 price = d.amount,
                                                 quantity = d.quantity,
                                                 total_amount = d.total_amount,
                                                 product_avatar = pr.avatar
                                             }).ToList()
                           });

            if (request.order_code != null && request.order_code.Length > 0)
            {
                lstData = lstData.Where(x => x.order_code.Trim().ToLower().Contains(request.order_code.Trim().ToLower())
                || x.customer_phone.Trim().ToLower().Contains(request.order_code.Trim().ToLower()) || x.customer_name.Trim().ToLower().Contains(request.order_code.Trim().ToLower()));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.order_date >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.order_date <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.partner_id == request.partner_id);
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");

            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã đơn hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Khách hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày mua</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Giá trị</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số điện thoại</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].order_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].customer_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].order_date_2 + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].total_amount) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_name + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].customer_phone + "</td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Thu hồi điểm
        [Route("recallpointorder")]
        [HttpPost]
        public ActionResult ExportRecallPointOrder(UserRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_business_recallpointorder, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            var lstData = (from p in _context.Users
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           where p.is_violation == true
                           select new
                           {
                               id = p.id,
                               user_type = c != null ? "CUSTOMER" : "PARTNER",
                               user_type_name = c != null ? "Khách hàng" : "Đối tác",
                               customer_id = c != null ? c.id : null,
                               partner_id = s != null ? s.id : null,
                               username = p.username,
                               total_point = p.total_point,
                               point_waiting = p.point_waiting,
                               point_avaiable = p.point_avaiable,
                               point_affiliate = p.point_affiliate
                           });

            if (request.username != null)
            {
                lstData = lstData.Where(x => x.username.Trim().ToLower().Contains(request.username.Trim().ToLower()));
            }

            if (request.user_type != null)
            {
                lstData = lstData.Where(x => x.user_type == request.user_type);
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tích điểm đội nhóm</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điểm khả dụng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điểm chờ</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tổng điểm</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].user_type_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].username + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].point_affiliate + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].point_avaiable + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].point_waiting + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].total_point + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Sản phẩm
        [Route("product")]
        [HttpPost]
        public ActionResult ExportProduct(ProductRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_business_product, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            var lstProduct = (from p in _context.Products
                              join g in _context.ProductGroups on p.product_group_id equals g.id into gs
                              from g in gs.DefaultIfEmpty()
                              join s in _context.Partners on p.partner_id equals s.id
                              orderby p.date_created descending
                              join st in _context.OtherLists on p.status equals st.id into sts
                              from st in sts.DefaultIfEmpty()
                              where p.status_change == true
                              orderby p.date_created descending
                              select new
                              {
                                  id = p.id,
                                  code = p.code,
                                  name = p.name,
                                  price = p.price,
                                  product_group_id = p.product_group_id,
                                  product_group_name = g != null ? g.name : "",
                                  partner_id = p.partner_id,
                                  partner_name = s.name,
                                  status = p.status,
                                  status_name = st != null ? st.name : "",
                                  description = p.description
                              });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstProduct = lstProduct.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.description.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            if (request.product_group_id != null)
            {
                lstProduct = lstProduct.Where(x => x.product_group_id == request.product_group_id);
            }

            if (request.status != null)
            {
                lstProduct = lstProduct.Where(x => x.status == request.status);
            }

            if (request.partner_id != null)
            {
                lstProduct = lstProduct.Where(x => x.partner_id == request.partner_id);
            }

            if (request.list_status_not_in != null && request.list_status_not_in.Count > 0)
            {
                lstProduct = lstProduct.Where(x => request.list_status_not_in.Contains((int)x.status) == false);
            }

            // Đếm số lượng
            int countElements = lstProduct.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            var dataR = lstProduct.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstProduct.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Nhóm sản phẩm</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã sản phẩm</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tên sản phẩm</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tên cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Giá</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái duyệt</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].product_group_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].price + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Đánh giá
        [Route("rating")]
        [HttpPost]
        public ActionResult ExportRating(AccumulatePointOrderRatingRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_business_rating, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            var lstData = (from p in _context.AccumulatePointOrderRatings
                           join ord in _context.AccumulatePointOrders on p.accumulate_point_order_id equals ord.id
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = ord.trans_no,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringFull(p.date_created),
                               partner_id = s.id,
                               partner_code = s.code,
                               partner_name = s.name,
                               customer_phone = c.phone,
                               customer_name = c.full_name,
                               content = p.content,
                               rating = p.rating,
                               rating_name = p.rating_name
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.partner_id == request.partner_id);
            }

            if (request.rating != null)
            {
                lstData = lstData.Where(x => x.rating == request.rating);
            }

            // Đếm số lượng
            //int countElements = lstData.Count();

            //// Số lượng trang
            //int totalPage = countElements > 0
            //        ? (int)Math.Ceiling(countElements / (double)request.page_size)
            //        : 0;

            //var dataR = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var lst = lstData.ToList();
            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã đơn mua</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày gửi đánh giá</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Khách hàng gửi đánh giá</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trung bình số sao đánh giá</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Nội dung đánh giá</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Đánh giá</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].date_created + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].customer_phone + "</td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].rating + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].content + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].rating_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }


        //Đối soát store
        [Route("StrAccumulatepointorder")]
        [HttpPost]
        [Authorize]
        public ActionResult StrAccumulatepointorder(AccumulatePointOrderRequest request)
        {
            var lstData = (from p in _context.AccumulatePointOrders
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                               date_created = p.date_created,
                               customer_phone = c != null ? c.phone : "",
                               partner_code = s.code,
                               partner_name = s.name,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_exchange,
                               point_customer = p.point_customer,
                               point_system = p.point_system,
                               point_partner = p.point_partner,
                               discount_rate = p.discount_rate,
                               description = p.description,
                               return_type = p.return_type,
                               payment_type = p.payment_type,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.payment_type != null)
            {
                lstData = lstData.Where(x => x.payment_type == request.payment_type);
            }

            if (request.return_type != null)
            {
                lstData = lstData.Where(x => x.return_type == request.return_type);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản khách hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã đối tác</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tổng hóa đơn(VNĐ)</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>% chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Hình thức thanh toán</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Loại chiết khấu</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái thanh toán</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_date + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].customer_phone + "</td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].bill_amount) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].discount_rate + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)lst[i].point_partner) + "</font></td>");
                var status_payment = lst[i].payment_type == "Cash" ? "Tiền mặt" : "Online";
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + status_payment + "</font></td>");
                var status_payment_return = lst[i].return_type == "Point" ? "Tích điểm" : "Hoàn tiền";
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + status_payment_return + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        public static string ReturnMoney(decimal number)
        {
            return number.ToString("#,##0");
        }


        //Quản lý khiếu nại
        [Route("Accumulatepointordercomplain")]
        [HttpPost]
        [Authorize]
        public ActionResult Accumulatepointordercomplain(AccumulatePointOrderComplainRequest request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_business_complain, (int)Enums.ActionType.Export))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            // Default page_no, page_size
            if (request.page_size < 1)
            {
                request.page_size = Consts.PAGE_SIZE;
            }

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.AccumulatePointOrderComplains
                           join b in _context.AccumulatePointOrders on p.accumulate_order_id equals b.id into bs
                           from b in bs.DefaultIfEmpty()
                           join c in _context.Customers on b.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on b.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               accumulate_order_id = p.accumulate_order_id,
                               customer_phone = c.phone,
                               trans_no = b.trans_no,
                               user_created = p.user_created,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringFull(p.date_created),
                               content = p.content,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()) || x.customer_phone.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.from_date != null && request.from_date.Length > 0)
            {
                lstData = lstData.Where(x => x.date_created_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null && request.to_date.Length > 0)
            {
                lstData = lstData.Where(x => x.date_created_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.user_created != null)
            {
                lstData = lstData.Where(x => x.user_created == request.user_created);
            }

            var lst = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản khách hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Nội dung khiếu nại</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < lst.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].trans_no + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + lst[i].customer_phone + "</td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].content + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + lst[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }
    }
}
