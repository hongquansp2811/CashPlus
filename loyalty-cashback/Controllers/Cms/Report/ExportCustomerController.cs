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
    [Route("api/cexport")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class ExportCustomerController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ILoggingHelpers _logging;
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public ExportCustomerController(IConfiguration configuration, ILoggingHelpers logging, LOYALTYContext context, ICommonFunction commonFunction)
        {
            _configuration = configuration;
            this._logging = logging;
            this._context = context;
            this._commonFunction = commonFunction;
        }

        // Đơn hàng
        [Route("order")]
        [HttpPost]
        public ActionResult ExportOrder(PartnerOrderRequest request)
        {
            //// Default page_no, page_size
            //if (request.page_size < 1)
            //{
            //    request.page_size = Consts.PAGE_SIZE;
            //}

            //if (request.page_no < 1)
            //{
            //    request.page_no = 1;
            //}

            // Số lượng Skip
            //int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.PartnerOrders
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           orderby p.date_created descending
                           select new
                           {
                               order_code = p.order_code,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               total_amount = p.total_amount,
                               partner_name = s.name,
                               status_name = st != null ? st.name : "",
                           });

            // Nếu tồn tại Where theo tên
            if (request.order_code != null && request.order_code.Length > 0)
            {
                lstData = lstData.Where(x => x.order_code.Contains(request.order_code));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            //// Đếm số lượng
            //int countElements = lstData.Count();

            //// Số lượng trang
            //int totalPage = countElements > 0
            //        ? (int)Math.Ceiling(countElements / (double)request.page_size)
            //        : 0;

            var dataResult = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border='1px'>");
            str.Append("<tr>");
            str.Append("<td><b><font face='Arial Narrow' size='4'>STT</font></b></td>");
            str.Append("<td><b><font face='Arial Narrow' size='4'>Mã đơn mua</font></b></td>");
            str.Append("<td><b><font face='Arial Narrow' size='4'>Ngày giao dịch</font></b></td>");
            str.Append("<td><b><font face='Arial Narrow' size='4'>Cửa hàng</font></b></td>");
            str.Append("<td><b><font face='Arial Narrow' size='4'>Giá trị đơn hàng</font></b></td>");
            str.Append("<td><b><font face='Arial Narrow' size='4'>Trạng thái</font></b></td>");
            str.Append("</tr>");

            for (int i = 0; i < dataResult.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face='Arial Narrow' size='17px'>" + (i + 1) + "</font></td>");
                str.Append("<td><font face='Arial Narrow' size='17px'>" + dataResult[i].order_code + "</font></td>");
                str.Append("<td><font face='Arial Narrow' size='17px'>" + dataResult[i].trans_date + "</font></td>");
                str.Append("<td><font face='Arial Narrow' size='17px'>" + dataResult[i].partner_name + "</font></td>");
                str.Append("<td><font face='Arial Narrow' size='17px'>" + ReturnMoney((decimal)dataResult[i].total_amount) + "</font></td>");
                str.Append("<td><font face='Arial Narrow' size='17px'>" + dataResult[i].status_name + "</font></td>");
                str.Append("</tr>");
            }

            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // LS Giao dịch
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

            // Số lượng Skip
            //int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.AccumulatePointOrders
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               partner_code = s.code,
                               partner_name = s.name,
                               address = s.address,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_exchange,
                               point_customer = p.point_customer,
                               point_partner = p.point_partner,
                               approve_user = p.user_created,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Contains(request.trans_no));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            // Đếm số lượng
            //int countElements = lstData.Count();

            //// Số lượng trang
            //int totalPage = countElements > 0
            //        ? (int)Math.Ceiling(countElements / (double)request.page_size)
            //        : 0;


            var dataResult = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>STT</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản đối tác</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số tiền thanh toán</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số điểm trả thưởng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản xác nhận</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < dataResult.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + (i + 1) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)dataResult[i].bill_amount) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)dataResult[i].point_partner) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].approve_user + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].status_name + "</font></td>");
                str.Append("</tr>");
            }

            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // LS Đổi điểm
        [Route("changepointorder")]
        [HttpPost]
        public ActionResult ExportChangePointOrder(ChangePointOrderRequest request)
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

            // Số lượng Skip
            //int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.ChangePointOrders
                           join u in _context.Users on p.user_id equals u.id into us
                           from u in us.DefaultIfEmpty()
                           join cb in _context.CustomerBankAccounts on p.customer_bank_account_id equals cb.id into cbs
                           from cb in cbs.DefaultIfEmpty()
                           join b in _context.Banks on cb.bank_id equals b.id into bs
                           from b in bs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.user_id == request.user_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               value_exchange = p.value_exchange,
                               point_exchange = p.point_exchange,
                               bank_name = b != null ? b.name : "",
                               bank_no = cb.bank_no,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Contains(request.trans_no));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }
            // Đếm số lượng
            //int countElements = lstData.Count();

            //// Số lượng trang
            //int totalPage = countElements > 0
            //        ? (int)Math.Ceiling(countElements / (double)request.page_size)
            //        : 0;

            var dataResult = lstData.ToList();
            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>STT</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số điểm quy đổi</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số tiền thanh toán</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngân hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Số tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Trạng thái</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < dataResult.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + (i + 1) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].trans_date + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)dataResult[i].point_exchange) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)dataResult[i].value_exchange) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].bank_name + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].bank_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].status_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // LS Đánh giá
        [Route("rating")]
        [HttpPost]
        public ActionResult ExportRating(AccumulatePointOrderRatingRequest request)
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

            // Số lượng Skip
            //int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.AccumulatePointOrderRatings
                           join ord in _context.AccumulatePointOrders on p.accumulate_point_order_id equals ord.id
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           select new
                           {
                               id = p.id,
                               trans_no = ord.trans_no,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringSort(p.date_created),
                               partner_code = s.code,
                               partner_name = s.name,
                               content = p.content,
                               rating = p.rating,
                               rating_name = p.rating_name
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Contains(request.trans_no));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
            }

            // Đếm số lượng
            //int countElements = lstData.Count();

            //// Số lượng trang
            //int totalPage = countElements > 0
            //        ? (int)Math.Ceiling(countElements / (double)request.page_size)
            //        : 0;

            var dataResult = lstData.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>STT</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Mã giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày giao dịch</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Cửa hàng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Nội dung đánh giá</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Đánh giá</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < dataResult.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + (i + 1) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].trans_no + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].date_created + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].partner_code + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].content + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].rating_name + "</font></td>");
                str.Append("</tr>");
            }
            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }

        // Team
        [Route("team")]
        [HttpPost]
        public ActionResult ExportTeam(CustomerRequest request)
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

            // Số lượng Skip
            //int skipElements = (request.page_no - 1) * request.page_size;
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var userObj = _context.Users.Where(x => x.customer_id == request.customer_id).FirstOrDefault();

            var from_date = request.from_date != null ? _commonFunction.convertStringSortToDate(request.from_date).Date : _commonFunction.convertStringSortToDate("01/01/2020");
            var to_date = request.to_date != null ? _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1) : _commonFunction.convertStringSortToDate("01/01/2900");

            var lstTeamLevel1 = (from p in _context.Users
                                 join sh in _context.Users on p.share_person_id equals sh.id
                                 join c in _context.Customers on p.customer_id equals c.id into cs
                                 from c in cs.DefaultIfEmpty()
                                 join st in _context.OtherLists on p.status equals st.id
                                 where p.share_person_id == userObj.id
                                 select new
                                 {
                                     avatar = p.avatar,
                                     full_name = c.full_name,
                                     share_code = p.share_code,
                                     phone = c.phone,
                                     share_person_name = sh.full_name,
                                     status = p.status,
                                     status_name = st.name,
                                     date_created = c.date_created,
                                     level = 1,
                                     total_point = p.total_point,
                                     count_person = _context.Users.Where(x => x.share_person_id == p.id).Count(),
                                     total_point_accumulate = (from tp in _context.CustomerPointHistorys
                                                               where tp.order_type == "AFF_LV_1" && tp.source_id == p.customer_id && tp.trans_date >= from_date && tp.trans_date <= to_date
                                                               select new
                                                               {
                                                                   point_amount = tp.point_amount
                                                               }).Sum(x => x.point_amount)
                                 });

            var idsLevel1 = _context.Users.Where(x => x.share_person_id == userObj.id).Select(x => x.id).ToList();
            var lstTeamLevel2 = (from p in _context.Users
                                 join sh in _context.Users on p.share_person_id equals sh.id
                                 join c in _context.Customers on p.customer_id equals c.id into cs
                                 from c in cs.DefaultIfEmpty()
                                 join st in _context.OtherLists on p.status equals st.id
                                 where idsLevel1.Contains(p.share_person_id)
                                 select new
                                 {
                                     avatar = p.avatar,
                                     full_name = c.full_name,
                                     share_code = p.share_code,
                                     phone = c.phone,
                                     share_person_name = sh.full_name,
                                     status = p.status,
                                     status_name = st.name,
                                     date_created = c.date_created,
                                     level = 2,
                                     total_point = p.total_point,
                                     count_person = _context.Users.Where(x => x.share_person_id == p.id).Count(),
                                     total_point_accumulate = (from tp in _context.CustomerPointHistorys
                                                               where tp.order_type == "AFF_LV_2" && tp.source_id == p.customer_id && tp.trans_date >= from_date && tp.trans_date <= to_date
                                                               select new
                                                               {
                                                                   point_amount = tp.point_amount
                                                               }).Sum(x => x.point_amount)
                                 });

            var idsLevel2 = _context.Users.Where(x => idsLevel1.Contains(x.share_person_id)).Select(x => x.id).ToList();
            var lstTeamLevel3 = (from p in _context.Users
                                 join sh in _context.Users on p.share_person_id equals sh.id
                                 join c in _context.Customers on p.customer_id equals c.id into cs
                                 from c in cs.DefaultIfEmpty()
                                 join st in _context.OtherLists on p.status equals st.id
                                 where idsLevel2.Contains(p.share_person_id)
                                 select new
                                 {
                                     avatar = p.avatar,
                                     full_name = c.full_name,
                                     share_code = p.share_code,
                                     phone = c.phone,
                                     share_person_name = sh.full_name,
                                     status = p.status,
                                     status_name = st.name,
                                     date_created = c.date_created,
                                     level = 3,
                                     total_point = p.total_point,
                                     count_person = _context.Users.Where(x => x.share_person_id == p.id).Count(),
                                     total_point_accumulate = (from tp in _context.CustomerPointHistorys
                                                               where tp.order_type == "AFF_LV_3" && tp.source_id == p.customer_id && tp.trans_date >= from_date && tp.trans_date <= to_date
                                                               select new
                                                               {
                                                                   point_amount = tp.point_amount
                                                               }).Sum(x => x.point_amount)
                                 });


            var lstResult = lstTeamLevel1.Concat(lstTeamLevel2).Concat(lstTeamLevel3);

            // Tìm kiếm
            if (request.search != null && request.search.Length > 0)
            {
                lstResult = lstResult.Where(x => x.full_name.Contains(request.search) || x.phone.Contains(request.search) || x.share_code.Contains(request.search));
            }

            if (request.level != null)
            {
                lstResult = lstResult.Where(x => x.level == request.level);
            }


            // Đếm số lượng
            //int countElements = lstResult.Count();

            //// Số lượng trang
            //int totalPage = countElements > 0
            //        ? (int)Math.Ceiling(countElements / (double)request.page_size)
            //        : 0;

            var dataResult = lstResult.ToList();

            StringBuilder str = new StringBuilder();
            str.Append("<html><head><meta charset='UTF-8'></head><body>");
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append("<td><b><font face=Arial Narrow size=3>STT</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tên người dùng</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tài khoản</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Ngày tham gia</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Điểm tích lũy theo thời gian</font></b></td>");
            str.Append("<td><b><font face=Arial Narrow size=3>Tổng số thành viên đã giới thiệu</font></b></td>");
            str.Append("</tr>");
            for (int i = 0; i < dataResult.Count; i++)
            {
                str.Append("<tr>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + (i + 1) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + dataResult[i].full_name + "</font></td>");
                str.Append("<td style='font-family: Arial Narrow; font-size: 14px; mso-number-format:\\@;'>" + dataResult[i].phone + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + _commonFunction.convertDateToStringSort(dataResult[i].date_created) + "</font></td>");
                var total_point_accumulate = dataResult[i].total_point_accumulate != null ? dataResult[i].total_point_accumulate : 0;

                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney((decimal)total_point_accumulate) + "</font></td>");
                str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + ReturnMoney(dataResult[i].count_person) + "</font></td>");
                str.Append("</tr>");
            }

            str.Append("</table></body></html>");

            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Export_" + DateTime.Now.Year.ToString() + ".html");
            this.Response.ContentType = "application/vnd.ms-word";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            return File(temp, "application/vnd.ms-word");
        }
        private static string ReturnMoney(decimal number)
        {
            return number.ToString("#,##0");
        }
    }
}
