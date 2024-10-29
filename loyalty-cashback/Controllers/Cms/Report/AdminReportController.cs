using DataObjects.Response;
using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace LOYALTY.Controllers
{
    [Route("api/report")]
    [Authorize(Policy = "WebAdminUser")]
    [ApiController]
    public class AdminReportController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILoggingHelpers _loggingHelpers;
        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ICommonFunction _commonFunction;

        private static string report_user = "cms_information_reporting_employer";
        private static string report_partner = "cms_information_reporting_partner";
        private static string cms_report_revenue = "cms_report_revenue";
        public AdminReportController(IDistributedCache distributedCache, ILoggingHelpers iLoggingHelpers, LOYALTYContext context, IEmailSender emailSender, ICommonFunction commonFunction)
        {
            _distributedCache = distributedCache;
            _loggingHelpers = iLoggingHelpers;
            _context = context;
            _emailSender = emailSender;
            _commonFunction = commonFunction;
        }

        // API Báo cáo doanh thu
        [Route("revenue")]
        [HttpPost]
        public JsonResult RevenueReport(AccumulatePointOrderRequest request)
        {
            var user2 = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Surname)).FirstOrDefault();
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, cms_report_revenue, (int)Enums.ActionType.View))
            {
                return new JsonResult(Consts.Error_Permissions) { StatusCode = 222 };
            }
            var username = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
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

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            var result = new APIResponse(dataResult);

            // Ghi log
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _loggingHelpers.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/report/revenue",
                actions = "Báo cáo tổng doanh thu",
                application = "WEB ADMIN",
                content = "Báo cáo tổng doanh thu",
                functions = "Báo cáo",
                is_login = false,
                result_logging = "Thành công",
                user_created = username.Value,
                IP = remoteIP.ToString()
            });
            return new JsonResult(result) { StatusCode = 200 };
        }


        //API báo cáo thông tin người tiêu dùng
        [Route("user")]
        [HttpPost]
        public JsonResult userReport(reportReq request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, report_user, (int)Enums.ActionType.View))
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

            var user = _context.Users.ToList();
            var AccumulatePointOrders = _context.AccumulatePointOrders.Where(l => l.status == 5).ToList();
            var AccumulatePointOrderRatings = _context.AccumulatePointOrderRatings.ToList();
            var ChangePointOrders = _context.ChangePointOrders.ToList();
            var AccumulatePointOrderComplains = (from p in _context.AccumulatePointOrderComplains
                                                 join b in _context.AccumulatePointOrders on p.accumulate_order_id equals b.id into bs
                                                 from b in bs.DefaultIfEmpty()
                                                 orderby p.date_created descending
                                                 select new
                                                 {
                                                     id = b.customer_id,
                                                 }).ToList();

            var lstData = (from p in _context.Customers
                           join u in _context.Users on p.id equals u.customer_id
                           where u.is_delete != true
                           select new UserReportRes
                           {
                               id = p.id,
                               name = p.full_name,
                               point_use = u.point_avaiable,
                               point_save = u.point_affiliate,
                               bank_acc = p.phone,
                               Date_join = p.date_created,
                               share_code = u.share_person_id
                           }).ToList();
            if (request.from_date != null)
            {
                lstData = lstData.Where(l => l.Date_join >= request.from_date).ToList();
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(l => l.Date_join <= request.to_date).ToList();
            }

            if (request.Key != null)
            {
                lstData = lstData.Where(l => l.name.Contains(request.Key) || l.bank_acc.Contains(request.Key)).ToList();
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();

            dataList.ForEach(item =>
            {
                var data = user.Where(l => l.share_person_id == item.id).ToList();
                item.count_introduce = data.Count();

                var AccumulatePointOrderTotal = AccumulatePointOrders.Where(p => p.customer_id == item.id && p.status == 5).ToList();
                item.count_transaction = AccumulatePointOrderTotal.Count();

                item.money_refund = AccumulatePointOrders.Where(x => x.customer_id == item.id && x.approve_date != null && x.point_customer != null && x.return_type == "Cash").Select(x => x.point_customer).Sum(x => x.Value);

                var countEvaluate = AccumulatePointOrderRatings.Where(x => x.customer_id == item.id).ToList();
                item.Evaluate = countEvaluate.Count();

                var Complain = AccumulatePointOrderComplains.Where(l => l.id == item.id).ToList();
                item.Complain = Complain.Count();

                item.total_changed = ChangePointOrders.Where(p => p.user_id == item.id).Select(x => x.point_exchange).Sum(x => x.Value);
            });

            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            APIResponse data = new APIResponse(dataResult);
            return new JsonResult(data) { StatusCode = 200 };
        }


        [Route("exportReportUser")]
        [HttpPost]
        public FileContentResult exportReportUser(reportReq request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, report_user, (int)Enums.ActionType.Export))
            {
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(Consts.Error_Permissions);

                var result = new FileContentResult(fileBytes, "application/octet-stream")
                {
                    FileDownloadName = "filename.ext"
                };

                Response.StatusCode = 222;

                return result;
            }
            var user = _context.Users.ToList();
            var AccumulatePointOrders = _context.AccumulatePointOrders.Where(l => l.status == 5).ToList();
            var AccumulatePointOrderRatings = _context.AccumulatePointOrderRatings.ToList();
            var ChangePointOrders = _context.ChangePointOrders.ToList();
            var AccumulatePointOrderComplains = (from p in _context.AccumulatePointOrderComplains
                                                 join b in _context.AccumulatePointOrders on p.accumulate_order_id equals b.id into bs
                                                 from b in bs.DefaultIfEmpty()
                                                 orderby p.date_created descending
                                                 select new
                                                 {
                                                     id = b.customer_id,
                                                 }).ToList();

            var lstData = (from p in _context.Customers
                           join u in _context.Users on p.id equals u.customer_id
                           where u.is_delete != true
                           select new UserReportRes
                           {
                               id = p.id,
                               name = p.full_name,
                               point_use = u.point_avaiable,
                               point_save = u.point_affiliate,
                               bank_acc = p.phone,
                               Date_join = p.date_created,
                               share_code = u.share_person_id
                           }).ToList();
            if (request.from_date != null)
            {
                lstData = lstData.Where(l => l.Date_join >= request.from_date).ToList();
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(l => l.Date_join <= request.to_date).ToList();
            }

            if (request.Key != null)
            {
                lstData = lstData.Where(l => l.name.Contains(request.Key) || l.bank_acc.Contains(request.Key)).ToList();
            }

            lstData.ForEach(item =>
            {
                var data = user.Where(l => l.share_person_id == item.id).ToList();
                item.count_introduce = data.Count();

                var AccumulatePointOrderTotal = AccumulatePointOrders.Where(p => p.customer_id == item.id && p.status == 5).ToList();
                item.count_transaction = AccumulatePointOrderTotal.Count();

                item.money_refund = AccumulatePointOrders.Where(x => x.customer_id == item.id && x.approve_date != null && x.point_customer != null && x.return_type == "Cash").Select(x => x.point_customer).Sum(x => x.Value);

                var countEvaluate = AccumulatePointOrderRatings.Where(x => x.customer_id == item.id).ToList();
                item.Evaluate = countEvaluate.Count();

                var Complain = AccumulatePointOrderComplains.Where(l => l.id == item.id).ToList();
                item.Complain = Complain.Count();

                item.total_changed = ChangePointOrders.Where(p => p.user_id == item.id).Select(x => x.point_exchange).Sum(x => x.Value);
            });

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("Sheet1");

                string reportTitle = "BÁO CÁO THÔNG TIN NGƯỜI TIÊU DÙNG";
                string datetime = $"{(request.from_date.HasValue ? request.from_date.Value.ToString("dd/MM/yyyy") : "")}" + "-" + $"{(request.to_date.HasValue ? request.to_date.Value.ToString("dd/MM/yyyy") : "")}";

                int datacol = 11;

                // Tạo tiêu đề cột
                worksheet.Cells["A2"].Value = "STT";
                worksheet.Cells["B2"].Value = "Ngày tham gia";
                worksheet.Cells["C2"].Value = "TT người dùng (Tài khoản-Họ tên)";
                worksheet.Cells["D2"].Value = "Điểm tiêu dùng";
                worksheet.Cells["E2"].Value = "Điểm tích lũy";
                worksheet.Cells["F2"].Value = "Tổng số tài khoản đã giới thiệu";
                worksheet.Cells["G2"].Value = "Tổng số giao dịch đã hoàn thành";
                worksheet.Cells["H2"].Value = "Đánh giá";
                worksheet.Cells["I2"].Value = "Khiếu nại";
                worksheet.Cells["J2"].Value = "Đổi điểm (SL giao dịch - Tổng điểm đã đổi";


                worksheet.Cells["A2:J2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A2:J2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A2:J2"].Style.Font.Size = 10;
                worksheet.Cells["A2:J2"].Style.Font.Bold = true;
                worksheet.Cells["A2:J2"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:J2"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:J2"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:J2"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:J2"].Style.WrapText = true;


                string rangeAddress1 = "A2:J2";
                // Set the background color for the range
                ExcelRange range = worksheet.Cells[rangeAddress1];
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepSkyBlue);


                string rangeAddress = "B2:J2";

                // Set the height of the range
                double height = 50; // Set the height to 20 (in points)
                for (int row = worksheet.Cells[rangeAddress].Start.Row; row <= worksheet.Cells[rangeAddress].End.Row; row++)
                {
                    worksheet.Row(row).Height = height;
                }

                // Set the width of the range
                double width = 22; // Set the width to 15 (in characters)
                for (int col = worksheet.Cells[rangeAddress].Start.Column; col <= worksheet.Cells[rangeAddress].End.Column; col++)
                {
                    worksheet.Column(col).Width = width;
                }

                int rowStart = 3;
                int k = 0;
                foreach (var item in lstData)
                {
                    for (int i = 1; i < datacol; i++)
                    {
                        if (i == 1)
                        {
                            worksheet.Cells[rowStart, i].Value = k + 1;
                        }
                        else if (i == 2)
                        {
                            var date = _commonFunction.convertDateToStringFull(item.Date_join);
                            worksheet.Cells[rowStart, i].Value = date != null ? date : "";
                        }
                        else if (i == 3)
                        {
                            string var = item.bank_acc + " - " + item.name;
                            worksheet.Cells[rowStart, i].Value = var;
                        }
                        else if (i == 4)
                        {
                            var point_use = item.point_use != null ? item.point_use : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)point_use);
                        }
                        else if (i == 5)
                        {
                            var point_save = item.point_save != null ? item.point_save : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)point_save);
                        }
                        else if (i == 6)
                        {
                            var count_introduce = item.count_introduce != null ? item.count_introduce : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)count_introduce);
                        }
                        else if (i == 7)
                        {
                            var count_transaction_completed = item.count_transaction_completed != null ? item.count_transaction_completed : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)count_transaction_completed);
                        }
                        else if (i == 8)
                        {
                            worksheet.Cells[rowStart, i].Value = item.Evaluate != null ? ReturnNumber((decimal)item.Evaluate) + "  đánh giá" : 0 + "  đánh giá";
                        }
                        else if (i == 9)
                        {
                            var Complain = item.Complain != null ? item.Complain : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)Complain);
                        }
                        else if (i == 10)
                        {
                            string key1 = item.count_transaction != null ? ReturnNumber((decimal)item.count_transaction) + "  giao dịch - " : 0 + "  giao dịch - ";
                            string key2 = item.total_changed != null ? ReturnNumber((decimal)item.total_changed) + "  điểm" : 0 + "  điểm";

                            worksheet.Cells[rowStart, i].Value = key1 + key2;
                        }
                        worksheet.Cells[rowStart, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.WrapText = true;
                    }

                    k++;
                    rowStart++;
                }

                // Convert package thành một mảng byte
                byte[] bytes = package.GetAsByteArray();

                // Xuất tệp Excel
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCaoThongTinNguoiTieuDung_" + DateTime.Now.ToString("ddd/MM/yyyy : HH/m/ss") + ".xlsx");
            }
        }


        //API báo cáo đối tác
        [Route("partnerReport")]
        [HttpPost]
        public JsonResult partnerReport(reportReq request)
        {

            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, report_partner, (int)Enums.ActionType.View))
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
            // Khai báo mảng ban đầu

            var AccumulatePointOrderComplains = (from p in _context.AccumulatePointOrderComplains
                                                 join b in _context.AccumulatePointOrders on p.accumulate_order_id equals b.id into bs
                                                 from b in bs.DefaultIfEmpty()
                                                 orderby p.date_created descending
                                                 select new
                                                 {
                                                     id = b.partner_id,
                                                 }).ToList();

            var user = _context.Users.ToList();
            var productData = _context.Products.Where(l => l.status == 5).ToList();
            var AccumulatePointOrders = _context.AccumulatePointOrders.Where(l => l.status == 5).ToList();
            var AccumulatePointOrderRatings = _context.AccumulatePointOrderRatings.ToList();
            var ChangePointOrders = _context.ChangePointOrders.ToList();

            var lstData = (from p in _context.Partners
                           join u in _context.Users.Where(x => x.is_partner_admin == true) on p.id equals u.partner_id
                           where p.is_delete != true && p.status != 14
                           orderby p.date_created descending
                           select new PartnerReportRes
                           {
                               partner_id = p.id,
                               Date_join = p.date_created,
                               Code = p.code,
                               name = p.name,
                               discount_percentage = p.discount_rate,
                               point_save = u.point_affiliate
                           }).ToList();

            if (request.from_date != null)
            {
                lstData = lstData.Where(l => l.Date_join >= request.from_date).ToList();
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(l => l.Date_join <= request.to_date).ToList();
            }

            if (request.Key != null)
            {
                lstData = lstData.Where(l => l.name.Contains(request.Key) || l.Code.Contains(request.Key)).ToList();
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();

            dataList.ForEach(item =>
            {
                item.count_introduce = user.Where(l => l.share_person_id == item.partner_id).Count();

                item.count_product = productData.Where(l => l.partner_id == item.partner_id).Count();

                item.count_epl = _context.Users.Where(x => x.partner_id == item.partner_id && x.is_delete != true && x.is_partner == true).Count();

                item.total_revenue = AccumulatePointOrders.Where(l => l.partner_id == item.partner_id).Select(p => p.bill_amount).Sum(x => x.Value);

                item.total_discount = item.total_revenue * (item.discount_percentage / 100);

                item.count_transaction = AccumulatePointOrders.Where(l => l.partner_id == item.partner_id).Count();

                item.Evaluate = AccumulatePointOrderRatings.Where(p => p.partner_id == item.partner_id).Count();

                item.Complain = AccumulatePointOrderComplains.Where(p => p.id == item.partner_id).Count();

                item.total_changed = ChangePointOrders.Where(p => p.user_id == item.partner_id).Select(x => x.point_exchange).Sum(x => x.Value);
                item.transaction = item.count_transaction;
            });

            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            APIResponse data = new APIResponse(dataResult);
            return new JsonResult(data) { StatusCode = 200 };
        }


        [Route("exportPartnerReport")]
        [HttpPost]
        public FileContentResult exportPartnerReport(reportReq request)
        {
            string all_permissions = User.Claims.Where(p => p.Type.Equals(ClaimTypes.Role)).Select(p => p.Value).FirstOrDefault();

            if (!CheckRole.Role(all_permissions, report_partner, (int)Enums.ActionType.Export))
            {
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(Consts.Error_Permissions);

                var result = new FileContentResult(fileBytes, "application/octet-stream")
                {
                    FileDownloadName = "filename.ext"
                };

                Response.StatusCode = 222;

                return result;
            }
            var AccumulatePointOrderComplains = (from p in _context.AccumulatePointOrderComplains
                                                 join b in _context.AccumulatePointOrders on p.accumulate_order_id equals b.id into bs
                                                 from b in bs.DefaultIfEmpty()
                                                 orderby p.date_created descending
                                                 select new
                                                 {
                                                     id = b.partner_id,
                                                 }).ToList();

            var user = _context.Users.ToList();
            var productData = _context.Products.Where(l => l.status == 5).ToList();
            var AccumulatePointOrders = _context.AccumulatePointOrders.Where(l => l.status == 5).ToList();
            var AccumulatePointOrderRatings = _context.AccumulatePointOrderRatings.ToList();
            var ChangePointOrders = _context.ChangePointOrders.ToList();

            var lstData = (from p in _context.Partners
                           join u in _context.Users.Where(x => x.is_partner_admin == true) on p.id equals u.partner_id
                           where p.is_delete != true && p.status != 14
                           orderby p.date_created descending
                           select new PartnerReportRes
                           {
                               partner_id = p.id,
                               Date_join = p.date_created,
                               Code = p.code,
                               name = p.name,
                               discount_percentage = p.discount_rate,
                               point_save = u.point_affiliate
                           }).ToList();

            if (request.from_date != null)
            {
                lstData = lstData.Where(l => l.Date_join >= request.from_date).ToList();
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(l => l.Date_join <= request.to_date).ToList();
            }

            if (request.Key != null)
            {
                lstData = lstData.Where(l => l.name.Contains(request.Key) || l.Code.Contains(request.Key)).ToList();
            }

            lstData.ForEach(item =>
            {
                item.count_introduce = user.Where(l => l.share_person_id == item.partner_id).Count();

                item.count_product = productData.Where(l => l.partner_id == item.partner_id).Count();

                item.count_epl = _context.Users.Where(x => x.partner_id == item.partner_id && x.is_delete != true && x.is_partner == true).Count();

                item.total_revenue = AccumulatePointOrders.Where(l => l.partner_id == item.partner_id).Select(p => p.bill_amount).Sum(x => x.Value);

                item.total_discount = item.total_revenue * (item.discount_percentage / 100);

                item.count_transaction = AccumulatePointOrders.Where(l => l.partner_id == item.partner_id).Count();

                item.Evaluate = AccumulatePointOrderRatings.Where(p => p.partner_id == item.partner_id).Count();

                item.Complain = AccumulatePointOrderComplains.Where(p => p.id == item.partner_id).Count();

                item.total_changed = ChangePointOrders.Where(p => p.user_id == item.partner_id).Select(x => x.point_exchange).Sum(x => x.Value);
                item.transaction = item.count_transaction;
            });


            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("Sheet1");

                string reportTitle = "BÁO CÁO THÔNG TIN ĐỐI TÁC";
                string datetime = $"{(request.from_date.HasValue ? request.from_date.Value.ToString("dd/MM/yyyy") : "")}" + "-" + $"{(request.to_date.HasValue ? request.to_date.Value.ToString("dd/MM/yyyy") : "")}";

                int datacol = 12;

                // Tạo tiêu đề cột
                worksheet.Cells["A2"].Value = "STT";
                worksheet.Cells["B2"].Value = "Ngày tham gia";
                worksheet.Cells["C2"].Value = "Đối tác (Mã - Tên - % chiết khấu)";
                worksheet.Cells["D2"].Value = "Tổng số tài khoản đã giới thiệu";
                worksheet.Cells["E2"].Value = "Tổng điểm tích lũy";
                worksheet.Cells["F2"].Value = "Tổng giao dịch";
                worksheet.Cells["G2"].Value = "Tổng số sản phẩm";
                worksheet.Cells["H2"].Value = "Số lượng nhân viên";
                worksheet.Cells["I2"].Value = "Khiếu nại";
                worksheet.Cells["J2"].Value = "Đánh giá";
                worksheet.Cells["K2"].Value = "Đổi điểm (SL giao dịch - Tổng điểm đã đổi";


                worksheet.Cells["A2:K2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A2:K2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A2:K2"].Style.Font.Size = 10;
                worksheet.Cells["A2:K2"].Style.Font.Bold = true;
                worksheet.Cells["A2:K2"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:K2"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:K2"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:L2"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:L2"].Style.WrapText = true;


                string rangeAddress1 = "A2:K2";
                // Set the background color for the range
                ExcelRange range = worksheet.Cells[rangeAddress1];
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepSkyBlue);


                string rangeAddress = "B2:K2";

                // Set the height of the range
                double height = 50; // Set the height to 20 (in points)
                for (int row = worksheet.Cells[rangeAddress].Start.Row; row <= worksheet.Cells[rangeAddress].End.Row; row++)
                {
                    worksheet.Row(row).Height = height;
                }

                // Set the width of the range
                double width = 22; // Set the width to 15 (in characters)
                for (int col = worksheet.Cells[rangeAddress].Start.Column; col <= worksheet.Cells[rangeAddress].End.Column; col++)
                {
                    worksheet.Column(col).Width = width;
                }

                int rowStart = 3;
                int k = 0;
                foreach (var item in lstData)
                {
                    for (int i = 1; i < datacol; i++)
                    {
                        if (i == 1)
                        {
                            worksheet.Cells[rowStart, i].Value = k + 1;
                        }
                        else if (i == 2)
                        {
                            var date = _commonFunction.convertDateToStringFull(item.Date_join);
                            worksheet.Cells[rowStart, i].Value = date != null ? date : "";
                        }
                        else if (i == 3)
                        {
                            string var = item.Code + " - " + item.name + " - " + item.discount_percentage + " %";
                            worksheet.Cells[rowStart, i].Value = var;
                        }
                        else if (i == 4)
                        {
                            worksheet.Cells[rowStart, i].Value = item.count_introduce != null ? item.count_introduce : 0;
                        }
                        else if (i == 5)
                        {
                            worksheet.Cells[rowStart, i].Value = item.point_save != null ? item.point_save : 0;
                        }
                        else if (i == 6)
                        {
                            string count1 = "SL giao dịch: " + (item.count_transaction != null ? ReturnNumber((decimal)item.count_transaction) + " giao dịch" : 0 + " giao dịch");
                            string count2 = "Tổng doanh thu: " + (item.total_revenue != null ? ReturnNumber((decimal)item.total_revenue) + " (VND)" : 0 + " (VND)");
                            string count3 = "Tổng chiết khấu (VND): " + (item.total_discount != null ? ReturnNumber((decimal)item.total_discount) + " (VND)" : 0 + " (VND)");
                            worksheet.Cells[rowStart, i].Value = count1 + count2 + count3;
                        }
                        else if (i == 7)
                        {
                            var count_pro = item.count_product != null ? item.count_product : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)count_pro);
                        }
                        else if (i == 8)
                        {
                            var count_epl = item.count_epl != null ? item.count_epl : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)count_epl);
                        }
                        else if (i == 9)
                        {
                            var Complain = item.Complain != null ? item.Complain : 0;
                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)Complain);
                        }
                        else if (i == 10)
                        {
                            var Evaluate = item.Evaluate != null ? item.Evaluate : 0;

                            worksheet.Cells[rowStart, i].Value = ReturnNumber((decimal)Evaluate);
                        }
                        else if (i == 11)
                        {
                            string key1 = item.count_transaction != null ? ReturnNumber((decimal)item.count_transaction) + "  giao dịch - " : 0 + "  giao dịch - ";
                            string key2 = item.total_changed != null ? ReturnNumber((decimal)item.total_changed) + "  điểm" : 0 + "  điểm";

                            worksheet.Cells[rowStart, i].Value = key1 + key2;
                        }
                        worksheet.Cells[rowStart, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowStart, i].Style.WrapText = true;
                    }
                    k++;
                    rowStart++;
                }
                // Convert package thành một mảng byte
                byte[] bytes = package.GetAsByteArray();

                // Xuất tệp Excel
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCaoThongTinDoiTac_" + DateTime.Now.ToString("ddd/MM/yyyy : HH/m/ss") + ".xlsx");
            }
        }

        public static string ReturnNumber(decimal number)
        {
            return number.ToString("#,##0");
        }
    }
}
