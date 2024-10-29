using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.Interfaces;
using LOYALTY.DataAccess;
using LOYALTY.PaymentGate;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Asn1.Ocsp;
using LOYALTY.CloudMessaging;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace LOYALTY.Controllers
{
    [Route("api/bktrans")]
    [ApiController]
    public class BKTransController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ICommon _common;
        private readonly ILoggingHelpers _logging;
        private readonly LOYALTYContext _context;
        private readonly IPartnerAccumulatePointOrder _partnerAccuPointOrder;
        private readonly IEmailSender _emailSender;
        private static JsonSerializerOptions option;
        private readonly BKTransaction _bkTransaction;
        private readonly FCMNotification _fCMNotification;

        //private readonly SysTransDataAccess _sysTransDataAccess;
        public BKTransController(IConfiguration configuration, ICommon common, ILoggingHelpers logging, LOYALTYContext context,
            IPartnerAccumulatePointOrder partnerAccuPointOrder, IEmailSender emailSender, BKTransaction bkTransaction, FCMNotification fCMNotification)
        {
            _configuration = configuration;
            _common = common;
            this._logging = logging;
            _context = context;
            _partnerAccuPointOrder = partnerAccuPointOrder;
            _emailSender = emailSender;
            _bkTransaction = bkTransaction;
            //_sysTransDataAccess = sysTransDataAccess;
            option = new JsonSerializerOptions { WriteIndented = true };
            _fCMNotification = fCMNotification;
        }



        [AllowAnonymous]
        [Route("callback")]
        [HttpPost]
        public JsonResult ReceiveVAEpay(BKCallbackRequest request)
        {
            try
            {
                // Check case thiếu param
                if (request.RequestId == null || request.RequestId.Length == 0)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "124",
                        ResponseMessage = "Thiếu trường RequestId"
                    })
                    { StatusCode = 200 };
                }

                if (request.RequestTime == null || request.RequestTime.Length == 0)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "124",
                        ResponseMessage = "Thiếu trường RequestTime"
                    })
                    { StatusCode = 200 };
                }

                if (request.TransAmount == null)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "124",
                        ResponseMessage = "Thiếu trường TransAmount"
                    })
                    { StatusCode = 200 };
                }

                if (request.Signature == null || request.Signature.Length == 0)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "124",
                        ResponseMessage = "Thiếu trường Signature"
                    })
                    { StatusCode = 200 };
                }

                // Check trùng ReferenceId
                var checkSameRequest = _context.BKVATransactions.Where(x => x.RequestId == request.RequestId).FirstOrDefault();

                if (checkSameRequest != null)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "102",
                        ResponseMessage = "Trùng RequestId"
                    })
                    { StatusCode = 200 };
                }

                // Check Sai PartnerCode
                if (BKConsts.PARTNER_CODE != request.PartnerCode)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "110",
                        ResponseMessage = "Sai PartnerCode"
                    })
                    { StatusCode = 200 };
                }

                // Check sai chữ ký
                string dataSign = request.RequestId + "|" + request.RequestTime + "|" + request.PartnerCode + "|" + request.AccNo + "|" + request.ClientIdNo + "|" + request.TransAmount.ToString() + "|" + request.TransTime
                    + "|" + request.BefTransDebt.ToString() + "|" + request.AffTransDebt.ToString() + "|" + request.AccountType.ToString() + "|" + request.OrderId;
                bool Signature = RSASign.verifySign256(dataSign, request.Signature, BKConsts.VA_EPAY_PUBLIC_KEY);
                if (Signature == false)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "103",
                        ResponseMessage = "Sai chữ ký"
                    })
                    { StatusCode = 200 };
                }

                var customerId = (from p in _context.Customers
                                  join f in _context.CustomerFakeBanks on p.id equals f.user_id
                                  where f.bank_account == request.AccNo
                                  select p.id).FirstOrDefault();

                // Check giao dịch thất bại
                if (customerId == null)
                {
                    return new JsonResult(new
                    {
                        ResponseCode = "11",
                        ResponseMessage = "Giao dịch thất bại"
                    })
                    { StatusCode = 200 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    ResponseCode = "11",
                    ResponseMessage = "Giao dịch thất bại"
                })
                { StatusCode = 200 };
            }

            var dateNow = DateTime.Now;

            // Lấy cấu hình đổi điểm
            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null || settingObj.change_point_estimate == null)
            {
                return new JsonResult(new
                {
                    ResponseCode = "11",
                    ResponseMessage = "Giao dịch thất bại"
                })
                { StatusCode = 200 };
            }
            // Cộng tiền vào tài khoản đối tác
            var partnerObj = (from p in _context.Partners
                              join f in _context.CustomerFakeBanks on p.id equals f.user_id
                              where f.bank_account == request.AccNo
                              select p).FirstOrDefault();

            var userPartnerObj = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == partnerObj.id).FirstOrDefault();

            decimal pointExchangeRate = Math.Round(((decimal)settingObj.point_exchange / (decimal)settingObj.point_value), 2);
            var amountIncrease = request.TransAmount;

            decimal value_exchange = (decimal)amountIncrease * pointExchangeRate;

            // Lấy config thưởng nạp điểm
            var bonusPointConfigObj = _context.BonusPointConfigs.Where(x => x.from_date <= dateNow && x.to_date >= dateNow && x.active == true && x.service_type_id == partnerObj.service_type_id && x.min_point <= value_exchange && x.max_point >= value_exchange).FirstOrDefault();

            var transaction = _context.Database.BeginTransaction();

            try
            {
                // Tạo log callback VA
                var newTrans = new BKVATransaction();
                newTrans.RequestId = request.RequestId;
                newTrans.RequestTime = request.RequestTime;
                newTrans.PartnerCode = request.PartnerCode;
                newTrans.AccNo = request.AccNo;
                newTrans.ClientIdNo = request.ClientIdNo;
                newTrans.TransId = request.TransId;
                newTrans.TransAmount = request.TransAmount;
                newTrans.TransTime = request.TransTime;
                newTrans.BefTransDebt = request.BefTransDebt;
                newTrans.AffTransDebt = request.AffTransDebt;
                newTrans.AccountType = request.AccountType;
                newTrans.OrderId = request.OrderId;
                newTrans.Signature = request.Signature;
                newTrans.date_created = DateTime.Now;
                _context.BKVATransactions.Add(newTrans);
                _context.SaveChanges();

                // Tạo giao dịch tiền đối tác
                var newAddPoint = new AddPointOrder();
                newAddPoint.id = Guid.NewGuid();
                var maxCodeObject = _context.AddPointOrders.Where(x => x.trans_no != null && x.trans_no.Contains("ND")).OrderByDescending(x => x.trans_no).FirstOrDefault();
                string codeOrder = "";
                if (maxCodeObject == null)
                {
                    codeOrder = "ND0000000000001";
                }
                else
                {
                    string maxCode = maxCodeObject.trans_no;
                    maxCode = maxCode.Substring(2);
                    int orders = int.Parse(maxCode);
                    orders = orders + 1;
                    string orderString = orders.ToString();
                    char pad = '0';
                    int number = 13;
                    codeOrder = "ND" + orderString.PadLeft(number, pad);
                }


                newAddPoint.trans_no = codeOrder;
                newAddPoint.partner_id = partnerObj.id;
                newAddPoint.status = 5;
                newAddPoint.bill_amount = amountIncrease;
                newAddPoint.point_avaiable = value_exchange;
                newAddPoint.point_waiting = bonusPointConfigObj != null ? Math.Round((value_exchange * (decimal)bonusPointConfigObj.discount_rate / 100)) : 0;
                newAddPoint.point_exchange = value_exchange + newAddPoint.point_waiting;
                newAddPoint.date_created = dateNow;
                newAddPoint.date_updated = dateNow;
                newAddPoint.approve_date = dateNow;
                newAddPoint.user_created = "Administrator";
                newAddPoint.user_updated = "Administrator";
                _context.AddPointOrders.Add(newAddPoint);

                _context.SaveChanges();

                // Cộng điểm đối tác
                userPartnerObj.point_avaiable += value_exchange;
                userPartnerObj.point_waiting += newAddPoint.point_waiting;
                userPartnerObj.total_point = userPartnerObj.point_avaiable + userPartnerObj.point_waiting + userPartnerObj.point_affiliate;

                _context.SaveChanges();
            }
            catch (System.Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new JsonResult(new
                {
                    ResponseCode = "11",
                    ResponseMessage = "Giao dịch thất bại"
                })
                { StatusCode = 200 };
            }
            transaction.Commit();
            transaction.Dispose();
            return new JsonResult(new
            {
                ResponseCode = "200",
                ResponseMessage = "Thành công"
            })
            { StatusCode = 200 };
        }

        //Webhock hứng dữ liệu từ BK
        [AllowAnonymous]
        [Route("pgcallback")]
        [HttpPost]
        public JsonResult BKPaymentGateCallback(OrderWebhookResponse response)
        {
            var remoteIP = Request.HttpContext.Connection.RemoteIpAddress;
            _logging.insertLogging(new LoggingRequest
            {
                user_type = Consts.USER_TYPE_WEB_ADMIN,
                is_call_api = true,
                api_name = "api/bktrans/pgcallback",
                actions = "Webhook nhận kết quả từ BK",
                application = "WEB ADMIN",
                content = "Dữ liệu nhận: " + JsonConvert.SerializeObject(response),
                functions = "Danh mục",
                is_login = false,
                result_logging = "Vào webhook",
                user_created = "Admin",
                IP = remoteIP.ToString()
            });
            try
            {
                var webhookData = new
                {
                    order = response.order,
                    txn = response.txn,
                    dataToken = response.dataToken
                };
                var orderId = response.order.mrc_order_id;
                var orderObj = _context.AccumulatePointOrders.Where(x => x.trans_no == orderId).FirstOrDefault();

                var data = JsonConvert.SerializeObject(webhookData);

                if (orderObj != null)
                {
                    orderObj.payment_gate_response = data;
                    orderObj.payment_gate_response_date = DateTime.Now;
                    _context.SaveChanges();
                }
                else
                {
                    return new JsonResult(new
                    {
                        err_code = "1",
                        message = "ERROR"
                    })
                    { StatusCode = 200 };
                }
                string clientSign = "";
                try
                {
                    clientSign = BaoKimApi.HmacSha256Encode(data);
                }
                catch (Exception ex)
                {
                    //Lấy partner
                    var partner= _context.Partners.Where(e=>e.id == orderObj.partner_id).FirstOrDefault();
                    if (partner != null)
                    {
                        var cfg = new ConfigAPI();
                        cfg.setValueCfg(partner.API_KEY, partner.API_SECRET);
                        clientSign = BaoKimApi.HmacSha256Encode(data);
                    }
                }

                if (response.sign == clientSign)
                {
                    // Xử lý lưu
                    if (orderObj != null)
                    {
                        //// Log giao dịch chuyển cho NTD
                        //BaoKimTransaction pgTrans = new BaoKimTransaction();
                        //pgTrans.id = Guid.NewGuid();
                        //pgTrans.payment_type = "PAYMENT_GATE";
                        //pgTrans.bao_kim_transaction_id = response.txn.reference_id;
                        //pgTrans.transaction_no = response.txn.mrc_order_id;
                        //pgTrans.accumulate_point_order_id = orderObj.id;
                        //pgTrans.partner_id = orderObj.partner_id;
                        //pgTrans.customer_id = orderObj.customer_id;
                        //Cập nhật lại BaoKimTransaction
                        var pgTrans = _context.BaoKimTransactions.Where(e => e.payment_type == "PAYMENT_GATE"
                        && e.accumulate_point_order_id == orderObj.id).FirstOrDefault();
                        pgTrans.bao_kim_transaction_id = response.txn.reference_id;
                        pgTrans.transaction_no = response.txn.mrc_order_id;
                        pgTrans.partner_id = orderObj.partner_id;
                        pgTrans.customer_id = orderObj.customer_id;
                        Guid customer_id = pgTrans.customer_id != null ? (Guid)pgTrans.customer_id : Guid.Empty;
                        if (response.order.stat == "c")
                        {
                            if (orderObj.status != 5)
                            {
                                _partnerAccuPointOrder.cashPaymentOnline((Guid)orderObj.id, "Administrator");
                            }
                            pgTrans.amount = decimal.Parse(response.txn.amount.ToString());
                            pgTrans.trans_status = 25;

                            //bắn socket thành công
                            // _ = _bkTransaction.pushSocket(pgTrans.transaction_no, "c");
                            _ = _bkTransaction.pushSocket(pgTrans.transaction_no, "c", customer_id);
                        }
                        else
                        {
                            orderObj.status = 4;
                            pgTrans.trans_status = 26;
                            //Bắn thông báo thất bại cho Partner
                            var notiConfig = _context.NotiConfigs.FirstOrDefault();
                            var Partner = _context.Users.Where(p => p.partner_id == orderObj.partner_id && p.is_partner == true).FirstOrDefault();
                            var Customer = _context.Users.Where(p => p.customer_id == orderObj.customer_id).FirstOrDefault();

                            if (Partner.device_id != null && Partner.send_Notification == true)
                            {
                                var newNoti1 = new Notification();
                                newNoti1.id = Guid.NewGuid();
                                newNoti1.title = "Hoàn tiền tiêu dùng";
                                newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                                newNoti1.user_id = Partner.partner_id;
                                newNoti1.date_created = DateTime.Now;
                                newNoti1.date_updated = DateTime.Now;
                                var returnNoti = notiConfig.MC_Payment_RefundFail;
                                newNoti1.content = returnNoti.Replace("{MaGiaoDich}", response.txn.mrc_order_id).Replace("{SoTienGiaoDich}", response.txn.amount.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }));
                                newNoti1.system_type = "ACCU_POINT";
                                newNoti1.reference_id = orderObj.id;

                                _context.Notifications.Add(newNoti1);
                                if (Partner.send_Notification == true)
                                {
                                    _fCMNotification.SendNotification(Partner.device_id,
                                                                         "ACCU_POINT",
                                                                         newNoti1.title,
                                                                 newNoti1.content,
                                                                         orderObj.id);
                                }
                            }

                            if (Customer.device_id != null && Customer.send_Notification == true)
                            {
                                var newNoti1 = new Notification();
                                newNoti1.id = Guid.NewGuid();
                                newNoti1.title = "Hoàn tiền tiêu dùng";
                                newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                                newNoti1.user_id = Customer.partner_id;
                                newNoti1.date_created = DateTime.Now;
                                newNoti1.date_updated = DateTime.Now;
                                var returnNoti = notiConfig.Payment_RefundFail;
                                newNoti1.content = returnNoti.Replace("{MaGiaoDich}", response.txn.mrc_order_id).Replace("{SoTienGiaoDich}", response.txn.amount.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }));
                                newNoti1.system_type = "ACCU_POINT";
                                newNoti1.reference_id = orderObj.id;

                                _context.Notifications.Add(newNoti1);
                                if (Customer.send_Notification == true)
                                {
                                    _fCMNotification.SendNotification(Customer.device_id,
                                                                      "ACCU_POINT",
                                                                      newNoti1.title,
                                                              newNoti1.content,
                                                                      orderObj.id);
                                }

                            }
                            _context.SaveChanges();

                            //bắn socket thất bại
                            // _ = _bkTransaction.pushSocket(pgTrans.transaction_no, "");
                            _ = _bkTransaction.pushSocket(pgTrans.transaction_no, "", customer_id);
                        }
                        //Lấy số dư
                        var partnerObj = _context.Partners.Where(x => x.id == orderObj.partner_id).FirstOrDefault();
                        if (partnerObj != null)
                        {
                            if (partnerObj.bk_partner_code != null && partnerObj.RSA_privateKey != null)
                            {
                                GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);
                                pgTrans.amount_balance = balanceObj.Available;
                            }
                        }

                        pgTrans.trans_log = System.Text.Json.JsonSerializer.Serialize(response, option);
                        pgTrans.transaction_date = DateTime.Now;
                        _context.BaoKimTransactions.Update(pgTrans);
                        _context.SaveChanges();
                    }
                    return new JsonResult(new
                    {
                        err_code = "0",
                        message = ""
                    })
                    { StatusCode = 200 };
                }
                else
                {

                    _context.SaveChanges();
                    return new JsonResult(new
                    {
                        err_code = "1",
                        message = "ERROR"
                    })
                    { StatusCode = 200 };
                }
            }
            catch (Exception ex)
            {
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_ADMIN,
                    is_call_api = true,
                    api_name = "api/bktrans/pgcallback",
                    actions = "Webhook nhận kết quả từ BK",
                    application = "WEB ADMIN",
                    content = "Lỗi: " + ex.Message + "/n/n"+ex.StackTrace,
                    functions = "Danh mục",
                    is_login = false,
                    result_logging = "Thất bại",
                    user_created = "Admin",
                    IP = remoteIP.ToString()
                });
                return new JsonResult(new
                {
                    err_code = "1",
                    message = "ERROR"
                })
                { StatusCode = 200 };
            }
        }
    }
}
