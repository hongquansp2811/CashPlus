using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using LOYALTY.PaymentGate.utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LOYALTY.PaymentGate
{

    public class BKTransaction
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly IEmailSender _emailSender;
        private readonly IJwtAuth _authen;
        private static readonly HttpClient client = new HttpClient();
        //private static string private_key = "";
        private static JsonSerializerOptions option;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILoggingHelpers _logging;

        public class BaoKimPGRequest
        {
            public string? mrc_order_id { get; set; }
            public long? total_amount { get; set; }
            public string? description { get; set; }
            public string? url_success { get; set; }
            public string? url_detail { get; set; }
            public string? merchant_id { get; set; }
            public string? lang { get; set; }
            public string? webhooks { get; set; }
            public string? customer_email { get; set; }
            public string? customer_phone { get; set; }
            public string? customer_name { get; set; }
            public string? customer_address { get; set; }
            public int? bpm_id { get; set; }
        }

        public BKTransaction(LOYALTYContext context, ICommonFunction commonFunction, IEmailSender emailSender, IJwtAuth authen, IHttpClientFactory clientFactory, ILoggingHelpers logging)
        {
            this._context = context;
            this._commonFunction = commonFunction;
            _emailSender = emailSender;
            _authen = authen;
            _clientFactory = clientFactory;
            //private_key = BKConsts.FB_PRIVATE_KEY;
            //private_key = "MIICWgIBAAKBgFoXsAEEvLG6mQSY4GzsBit6/pGeIDnmK/1eMZglpbImtwiW4KlS\r\nnoybya+eSH03CDNkAQIghTcctP9qbUg2bRFLNcNwDbSBSRUfKfm24HdrFLJ46ziZ\r\n94eruqswGqRQ8JbM8cY43h5c7eEmZB4vYw0pyO9i+jmSv+lIr2NZcrEfAgMBAAEC\r\ngYAYyXGcJiCASZV2BVWhwiJEbjeB+t5k76XktLiyYpE+/ZXYICK5k0iZ6PbJgaPy\r\nB2UTNo1sd2QPcK9/ollkx8yj5KJLR9yXnyjxohbSyLQXkNzxVzQIQRyu53Oh+WwQ\r\nkuC8kyDOKxeAGHzONXtR2WT13JiMtjKxOuGmt9Rt8SOYkQJBAKNdj7GY6EcOdikC\r\nF23Kef81H9CoUwqEyZ0eoEwdt2pMGyAsgGGng5TZqfXWiRQ33IG7FFMSdXoqrW3c\r\nWHO/8mkCQQCNLbAoJXCHDy2/gIlfH2KRIqBFVxRXBvPkoiCZIwSgUQlQUtIESB87\r\nlEimGmQoAZZDEuCaU+7H0+Tt4C3CSAZHAkB6TiyrMKgNsqUB9J/nwaPuTi6Af9ST\r\n1nA+4lPuSH0t5saUIt0Gv2wCf6b/91rvORcsRQxlWTd8fAEVc9cA6Z6pAkBzehmK\r\n3QTsFEhRSewTeHKBUJdT4GRswu0f6FVNrU0NbPt3TicnBW82ppW99/xQlOu5tWku\r\nEtVPckzhHeuP7KXlAkBB5NPqQqaxzetKz42mbh8sXWGXhsdJV2kfrRowelnsDm5M\r\nA5t+VK3oayGvursKRD8R3TkpJfJPBd12Mn5ocRUq";
            option = new JsonSerializerOptions { WriteIndented = true };
            _logging = logging;
        }

        public static string getVietNamDateTime(string format)
        {
            DateTime dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Asia/Ho_Chi_Minh");
            return dateTime.ToString(format);
        }

        public static string getRandomTransId(string prefix, int length)
        {
            string ran = getRandomString((length - 1) - prefix.Length);
            if (prefix.Equals(""))
            {
                return ran;
            }
            else
            {
                return prefix + "_" + ran;
            }
        }

        private static string getRandomString(int length)
        {
            string stock = "abcdefghijklmnopqrstuvwxyz0123456789";
            string ranStr = "";
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                ranStr += stock[random.Next(stock.Length - 1)];
            }
            return ranStr;
        }

        public async Task<string> createVirtualAccount(Guid partner_id , string private_key)
        {
            var partner = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (partner == null)
            {
                return "ERROR_PARTNER_NOT_FOUND";
            }

            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            Random rnd = new Random();
            int code = rnd.Next(100000000, 999999999);
            string partner_name = partner.name;
            partner_name = Strings.RemoveDiacritics(partner_name);

            var request_data_before_encrypt = new
            {
                RequestId = BKConsts.PARTNER_CODE + "BK" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + code,
                RequestTime = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0') + " " + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0'),
                PartnerCode = BKConsts.PARTNER_CODE,
                Operation = "9001",
                CreateType = 2,
                AccName = partner_name,
                CollectAmountMin = 50000,
                CollectAmountMax = 50000000,
                ExpireDate = "2025-12-30 23:00:00",
                AccNo = "",
                OrderId = partner_id
            };

            string jsonStringRequestData = JsonConvert.SerializeObject(request_data_before_encrypt);
            string signData = RSASign.sign(jsonStringRequestData, private_key);

            client.DefaultRequestHeaders.TryAddWithoutValidation("Signature", signData);

            try
            {
                var responseOcdFront = await client.PostAsync(BKConsts.URL_REGISTER_VA, new StringContent(JsonConvert.SerializeObject(request_data_before_encrypt), Encoding.UTF8, "application/json"));
                responseOcdFront.EnsureSuccessStatusCode();

                var contentOcd = await responseOcdFront.Content.ReadAsStringAsync();
                JObject dataResponseOcd = (JObject)JsonConvert.DeserializeObject(contentOcd);

                var responseCode = dataResponseOcd["ResponseCode"];

                if (responseCode.ToString() == "200")
                {
                    try
                    {
                        string account_no = dataResponseOcd["AccountInfo"]["BANK"]["AccNo"].ToString();
                        string account_name = dataResponseOcd["AccountInfo"]["BANK"]["AccName"].ToString();
                        string bank_name = dataResponseOcd["AccountInfo"]["BANK"]["BankName"].ToString();

                        var newCusFake = new CustomerFakeBank();
                        var oldId = (from p in _context.CustomerFakeBanks
                                     orderby p.id descending
                                     select p.id).FirstOrDefault();

                        newCusFake.id = Guid.NewGuid();
                        newCusFake.supplier = "BAOKIM";
                        newCusFake.date_created = DateTime.Now;
                        newCusFake.bank_account = account_no;
                        newCusFake.bank_owner = account_name;
                        newCusFake.bank_name = bank_name;
                        newCusFake.user_id = partner_id;

                        _context.CustomerFakeBanks.Add(newCusFake);

                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return "ERROR_SAVE_VA";
                    }
                }
                else
                {
                    return contentOcd;
                }
            }
            catch (Exception ex)
            {
                return ex.InnerException.Message;
            }

            return null;
        }

        public TransferResponseObj transferMoney(string partnerCode, string bankNo, string accNo, string accName, decimal amount, string memo, string private_key)
        {
            string result_req = "";

            string passPhrase = "Pas5pr@se";
            string saltValue = "s@1tValue";
            string hashAlgorithm = "SHA1";
            int passwordIterations = 2;
            string initVector = "@CSS@CSS@CSS@CSS";
            int keySize = 256;
            string dePartner_code = EncryptData.Decrypt(partnerCode, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);

            try
            {
                Random rnd = new Random();
                int code = rnd.Next(100000000, 999999999);

                RequestObject request = new RequestObject();
                request.RequestId = dePartner_code + "BK" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + code.ToString();
                request.RequestTime = DataGeneratorUtil.getVietNamDateTime("yyyy-MM-dd HH:mm:ss");
                request.PartnerCode = dePartner_code;
                request.Operation = 9002;
                request.BankNo = bankNo;
                request.AccNo = accNo;
                request.AccType = 0;
                request.Memo = memo != null ? memo : ("LOYALTY: " + request.AccNo);
                request.RequestAmount = amount;
                request.ReferenceId = DataGeneratorUtil.getRandomTransId("REQID", 25);


                string dataSign = request.RequestId + "|" + request.RequestTime + "|" + request.PartnerCode + "|" + request.Operation + "|" + request.ReferenceId + "|" + request.BankNo + "|" + request.AccNo + "|" + request.AccType + "|" + request.RequestAmount + "|" + request.Memo;
                request.Signature = RSASign.sign(dataSign, private_key);

                string json_request = System.Text.Json.JsonSerializer.Serialize(request, option);
                Console.WriteLine("Request:\n" + json_request + "\n");

                Task<string> response = HttpsRequestUtil.postToAddress(BKConsts.API_URL, json_request);
                result_req = response.Result;

                TransferResponseObj result = (TransferResponseObj)System.Text.Json.JsonSerializer.Deserialize(response.Result, typeof(TransferResponseObj));

                return result;
            }
            catch (Exception ex)
            {
                TransferResponseObj result = new TransferResponseObj();
                result.ResponseCode = 999;
                result.ResponseMessage = ex.Message + " :" + result_req;
                return result;
            }

        }

        public GetBalanceResponseObj getBalanceFirmBank(string partnerCode, string private_key)
        {

            string passPhrase = "Pas5pr@se";
            string saltValue = "s@1tValue";
            string hashAlgorithm = "SHA1";
            int passwordIterations = 2;
            string initVector = "@CSS@CSS@CSS@CSS";
            int keySize = 256;
            string dePartner_code = EncryptData.Decrypt(partnerCode, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
            string result_req = "";
            var requestEx= new GetBalanceRequestObject();  
            var str  = "";
            try
            {
                Random rnd = new Random();
                int code = rnd.Next(100000000, 999999999);

                GetBalanceRequestObject request = new GetBalanceRequestObject();
                GetBalanceResponseObj  result  = new GetBalanceResponseObj();
                request.RequestId = dePartner_code + "BK" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + code.ToString();
                request.RequestTime = DataGeneratorUtil.getVietNamDateTime("yyyy-MM-dd HH:mm:ss");
                request.PartnerCode = dePartner_code;
                request.Operation = 9004;

                string dataSign = request.RequestId + "|" + request.RequestTime + "|" + request.PartnerCode + "|" + request.Operation;
                request.Signature = RSASign.sign(dataSign, private_key);
                result.bodyRequest = request != null ? request : null; 
                requestEx = request != null ? request : null;
                string json_request =  System.Text.Json.JsonSerializer.Serialize(request, option);
                str  =  json_request.ToString() != null ? json_request.ToString() : "";
                result.body = json_request.ToString() != null ? json_request.ToString() : "";

                Console.WriteLine("Request:\n" + json_request + "\n");
                Task<string> response = HttpsRequestUtil.postToAddress(BKConsts.API_URL, json_request);
                result_req = response.Result;

                result = (GetBalanceResponseObj)System.Text.Json.JsonSerializer.Deserialize(response.Result, typeof(GetBalanceResponseObj));
                result.body = json_request.ToString() != null ? json_request.ToString() : "";
                result.bodyRequest = request != null ? request : null; 

                return result;
            }
            catch (Exception ex)
            {
                GetBalanceResponseObj result = new GetBalanceResponseObj();
                result.ResponseCode = 999;
                result.ResponseMessage = ex.Message + " :" + result_req;
                result.body = str;
                result.bodyRequest = requestEx; 
                return result;
            }
        }

        public async Task<string> createPaymentLink(Guid order_id)
        {
            var result_link = "Success +/";

            var orderObj = (from p in _context.AccumulatePointOrders
                            join s in _context.Partners on p.partner_id equals s.id
                            join c in _context.Customers on p.customer_id equals c.id
                            where p.id == order_id
                            select new
                            {
                                id = p.id,
                                trans_no = p.trans_no,
                                partner_code = s.code,
                                bill_amount = p.bill_amount,
                                customer_name = c.full_name,
                                customer_phone = c.phone,
                                customer_email = c.email,
                                customer_address = c.address,
                                bk_merchant_id = s.bk_merchant_id,
                                API_KEY = s.API_KEY,
                                API_SECRET = s.API_SECRET
                            }).FirstOrDefault();
            if (orderObj == null)
            {
                return "Error +/ Kiểm tra lại thông tin đơn hàng!";
            }
            string de_bk_merchant_id = "";
            if (orderObj.bk_merchant_id != null)
            {
                string passPhrase = "Pas5pr@se";
                string saltValue = "s@1tValue";
                string hashAlgorithm = "SHA1";
                int passwordIterations = 2;
                string initVector = "@CSS@CSS@CSS@CSS";
                int keySize = 256;
                de_bk_merchant_id = EncryptData.Decrypt(orderObj.bk_merchant_id, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
            }

            if (orderObj.API_KEY == null || orderObj.API_SECRET == null || orderObj.bk_merchant_id == null)
            {
                return "Error +/ Kiểm tra lại thông tin kết nối công thanh toán!";
            }
            BaoKimPGRequest request = new BaoKimPGRequest();
            request.bpm_id = 295;
            //request.mrc_order_id = DateTime.Now.ToString("yyMMddHHmmss");
            request.mrc_order_id = orderObj.trans_no;
            request.total_amount = long.Parse(orderObj.bill_amount.ToString());
            request.description = "Don hang cua merchant " + orderObj.partner_code;
            request.url_success = BKConsts.PG_BASE_URL_RETURN;
            request.url_detail = BKConsts.PG_BASE_URL_RETURN;
            request.lang = "en";
            request.merchant_id =  de_bk_merchant_id;
            request.webhooks = BKConsts.PG_BASE_URL_IPN;
            request.customer_name = Strings.RemoveDiacritics(orderObj.customer_name);
            if (orderObj.customer_phone != null) request.customer_phone = orderObj.customer_phone;
            else request.customer_phone = "0123456788";
            //if (orderObj.customer_email != "") request.customer_email = orderObj.customer_email;
            //else request.customer_email = "testpayment@gmail.com";
            if (orderObj.customer_email != null && orderObj.customer_email != "") request.customer_email = orderObj.customer_email;
            else request.customer_email = "info@cashplus.vn";
            if (orderObj.customer_address != null) request.customer_address = (orderObj.customer_address != null && orderObj.customer_address.Length > 0) ? Strings.RemoveDiacritics(orderObj.customer_address) : "";

            try
            {
                string api_key = orderObj.API_KEY;
                string api_secret = orderObj.API_SECRET;

                //BaoKimApi.API_KEY = api_key;
                //BaoKimApi.API_SECRET = api_secret;

                var cfg = new ConfigAPI();
                cfg.setValueCfg(api_key, api_secret);

                string token = BaoKimApi.JWT;  
                var client = _clientFactory.CreateClient("HttpClientWithSSLUntrusted");

                string bodyString = JsonConvert.SerializeObject(request);

                JObject Object = JObject.Parse(bodyString);

                RemoveNullProperties(Object);

                string cleanedJsonString = Object.ToString();

                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("jwt", "Bearer " + token);

                var responseSend = await client.PostAsync(BKConsts.PG_BASE_URL_SEND + "?jwt=" + token, new StringContent(cleanedJsonString, Encoding.UTF8, "application/json"));
                responseSend.EnsureSuccessStatusCode();

                var content2 = await responseSend.Content.ReadAsStringAsync();
                JObject dataResponse2 = (JObject)JsonConvert.DeserializeObject(content2);

                if (dataResponse2 != null && dataResponse2["code"] != null && dataResponse2["code"].ToString() == "0")
                {
                    var objData = dataResponse2["data"];
                    //result_link += objData["data_qr"].ToString();
                    result_link += objData["bank_account"]["Qr"].ToString();
                    //
                    // Log giao dịch chuyển cho NTD
                    BaoKimTransaction pgTrans = new BaoKimTransaction();
                    pgTrans.id = Guid.NewGuid();
                    pgTrans.payment_type = "PAYMENT_GATE";
                    //pgTrans.bao_kim_transaction_id = response.txn.reference_id;
                    //pgTrans.transaction_no = response.txn.mrc_order_id;
                    pgTrans.accumulate_point_order_id = orderObj.id;
                    //pgTrans.partner_id = orderObj.partner_id;
                    //pgTrans.customer_id = orderObj.customer_id;
                    pgTrans.bank_receive_name = objData["bank_account"]["BankShortName"].ToString() + "-" + objData["bank_account"]["BankName"].ToString();
                    pgTrans.bank_receive_account = objData["bank_account"]["AccNo"].ToString();
                    pgTrans.bank_receive_owner = objData["bank_account"]["AccName"].ToString();
                    pgTrans.trans_status = 27;
                    pgTrans.transaction_date = DateTime.Now;
                    _context.BaoKimTransactions.Add(pgTrans);
                    _context.SaveChanges();
                }
                else
                {
                    //result_link = JsonConvert.SerializeObject(request) + "??" + content2.ToString();

                    // Deserialize the JSON string
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(content2.ToString());
                    var text = jsonObject.message;
                    string result = text.ToString();

                    var textReplace = result.Replace("\r\n", "").Replace("\"","").Replace("[","").Replace("]","");

                    result_link = "Error +/" + textReplace;
                }

            }
            catch (Exception ex)
            {
                result_link = "Error +/" + ex.Message;
            }

            return result_link;
        }

        public async Task<string> createPaymentLinkFull(Guid order_id)
        {
            var result_link = "Success +/";

            var orderObj = (from p in _context.AccumulatePointOrders
                            join s in _context.Partners on p.partner_id equals s.id
                            join c in _context.Customers on p.customer_id equals c.id
                            where p.id == order_id
                            select new
                            {
                                id = p.id,
                                trans_no = p.trans_no,
                                partner_code = s.code,
                                bill_amount = p.bill_amount,
                                customer_name = c.full_name,
                                customer_phone = c.phone,
                                customer_email = c.email,
                                customer_address = c.address,
                                bk_merchant_id = s.bk_merchant_id,
                                API_KEY = s.API_KEY,
                                API_SECRET = s.API_SECRET
                            }).FirstOrDefault();
            if (orderObj == null)
            {
                return "Error +/ Kiểm tra lại thông tin đơn hàng!";
            }
            string de_bk_merchant_id = "";
            if (orderObj.bk_merchant_id != null)
            {
                string passPhrase = "Pas5pr@se";
                string saltValue = "s@1tValue";
                string hashAlgorithm = "SHA1";
                int passwordIterations = 2;
                string initVector = "@CSS@CSS@CSS@CSS";
                int keySize = 256;
                de_bk_merchant_id = EncryptData.Decrypt(orderObj.bk_merchant_id, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
            }

            if (orderObj.API_KEY == null || orderObj.API_SECRET == null || orderObj.bk_merchant_id == null)
            {
                return "Error +/ Kiểm tra lại thông tin kết nối công thanh toán!";
            }
            BaoKimPGRequest request = new BaoKimPGRequest();
            request.bpm_id = 295;
            //request.mrc_order_id = DateTime.Now.ToString("yyMMddHHmmss");
            request.mrc_order_id = orderObj.trans_no;
            request.total_amount = long.Parse(orderObj.bill_amount.ToString());
            request.description = "Don hang cua merchant " + orderObj.partner_code;
            request.url_success = BKConsts.PG_BASE_URL_RETURN;
            request.url_detail = BKConsts.PG_BASE_URL_RETURN;
            request.lang = "en";
            request.merchant_id = de_bk_merchant_id;
            request.webhooks = BKConsts.PG_BASE_URL_IPN;
            request.customer_name = Strings.RemoveDiacritics(orderObj.customer_name);
            if (orderObj.customer_phone != null) request.customer_phone = orderObj.customer_phone;
            else request.customer_phone = "0123456788";
            //if (orderObj.customer_email != "") request.customer_email = orderObj.customer_email;
            //else request.customer_email = "testpayment@gmail.com";
            if (orderObj.customer_email != null && orderObj.customer_email != "") request.customer_email = orderObj.customer_email;
            else request.customer_email = "info@cashplus.vn";
            if (orderObj.customer_address != null) request.customer_address = (orderObj.customer_address != null && orderObj.customer_address.Length > 0) ? Strings.RemoveDiacritics(orderObj.customer_address) : "";

            try
            {
                string api_key = orderObj.API_KEY;
                string api_secret = orderObj.API_SECRET;

                //BaoKimApi.API_KEY = api_key;
                //BaoKimApi.API_SECRET = api_secret;

                var cfg = new ConfigAPI();
                cfg.setValueCfg(api_key, api_secret);

                string token = BaoKimApi.JWT;
                var client = _clientFactory.CreateClient("HttpClientWithSSLUntrusted");

                string bodyString = JsonConvert.SerializeObject(request);

                JObject Object = JObject.Parse(bodyString);

                RemoveNullProperties(Object);

                string cleanedJsonString = Object.ToString();
                result_link += " - Request:" + bodyString;
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("jwt", "Bearer " + token);

                var responseSend = await client.PostAsync(BKConsts.PG_BASE_URL_SEND + "?jwt=" + token, new StringContent(cleanedJsonString, Encoding.UTF8, "application/json"));
                responseSend.EnsureSuccessStatusCode();

                var content2 = await responseSend.Content.ReadAsStringAsync();
                JObject dataResponse2 = (JObject)JsonConvert.DeserializeObject(content2);

                if (dataResponse2 != null && dataResponse2["code"] != null && dataResponse2["code"].ToString() == "0")
                {
                    //var objData = dataResponse2["data"];
                    //result_link += objData["data_qr"].ToString();
                    result_link += " - Response:"+ content2.ToString();
                }
                else
                {
                    //result_link = JsonConvert.SerializeObject(request) + "??" + content2.ToString();

                    // Deserialize the JSON string
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(content2.ToString());
                    var text = jsonObject.message;
                    string result = text.ToString();

                    var textReplace = result.Replace("\r\n", "").Replace("\"", "").Replace("[", "").Replace("]", "");

                    result_link += "Error :" + textReplace;
                }

            }
            catch (Exception ex)
            {
                result_link += "Error +/" + ex.Message;
            }

            return result_link;
        }

        public async Task pushSocket(string mrc_order_id, string stat, Guid customer_id)
        {
            // string url = BKConsts.PG_BASE_URL_RETURN + "?mrc_order_id=" + mrc_order_id + "&stat="+ stat;
            string url = BKConsts.PG_BASE_URL_RETURN + "?mrc_order_id=" + mrc_order_id + "&stat=" + stat + "&customer_id=" + customer_id;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage responseMessage = await client.GetAsync(url);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Yêu cầu đã được gửi thành công.");
                        //_logging.insertLogging(new LoggingRequest
                        //{
                        //    user_type = Consts.USER_TYPE_WEB_ADMIN,
                        //    is_call_api = true,
                        //    api_name = "api/bktrans/pgcallback",
                        //    actions = "Webhook nhận kết quả từ BK",
                        //    application = "WEB ADMIN",
                        //    content = "Dữ liệu nhận: " + JsonConvert.SerializeObject(response),
                        //    functions = "Danh mục",
                        //    is_login = false,
                        //    result_logging = "Vào webhook",
                        //    user_created = "Admin",
                        //    IP = remoteIP.ToString()
                        //});
                    }
                    else
                    {
                        Console.WriteLine($"Yêu cầu không thành công. Mã trạng thái: {responseMessage.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi: {ex.Message}");
                }
            }
        }
        public async Task<string> checkAccountExist(string bankNo, string accNo, string accName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string passPhrase = "Pas5pr@se";
            string saltValue = "s@1tValue";
            string hashAlgorithm = "SHA1";
            int passwordIterations = 2;
            string initVector = "@CSS@CSS@CSS@CSS";
            int keySize = 256;
            string dePartner_code = EncryptData.Decrypt(Consts.CP_BK_PARTNER_CODE, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);

            Random rnd = new Random();
            int code = rnd.Next(100000000, 999999999);
            var requestData = new BKFirmBankinkRequest
            {
                RequestId = dePartner_code + "BK" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + code,
                RequestTime = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0') + " " + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0'),
                PartnerCode = dePartner_code,
                Operation = "9001",
                BankNo = bankNo,
                AccNo = accNo,
                AccType = 0
            };

            string dataSign = requestData.RequestId + "|" + requestData.RequestTime + "|" + requestData.PartnerCode + "|" + requestData.Operation + "|" + requestData.BankNo + "|" + requestData.AccNo + "|" + requestData.AccType;
            string signData = RSASign.sign(dataSign, Consts.private_key);

            requestData.Signature = signData;

            client.DefaultRequestHeaders.TryAddWithoutValidation("Signature", signData);

            try
            {
                var response = await HttpsRequestUtil.postToAddress(BKConsts.API_URL, System.Text.Json.JsonSerializer.Serialize(requestData, option));
                var dataResponseOcd = JsonConvert.DeserializeObject<BKFirmBankResponse>(response);

                if (dataResponseOcd.ResponseCode == 200)
                {
                    try
                    {
                        if (!dataResponseOcd.AccName.Equals(accName.ToUpper()))
                        {
                            return "ERR_BANK_ACC_NOT_EXIST";
                        }
                    }
                    catch (Exception ex)
                    {
                        return "ERR_BANK_ACC_NOT_EXIST";
                    }
                }
                else
                {
                    return "ERR_BANK_ACC_NOT_EXIST";
                }
            }
            catch (Exception ex)
            {
                return ex.InnerException.Message;
            }

            return null;
        }

        void RemoveNullProperties(JObject obj)
        {
            var propertiesToRemove = obj.Properties()
                .Where(p => p.Value.Type == JTokenType.Null)
                .ToList();

            foreach (var property in propertiesToRemove)
            {
                property.Remove();
            }

            foreach (var property in obj.Properties())
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    RemoveNullProperties(property.Value as JObject);
                }
            }
        }

        public CheckVerifyConnect checkVerifyConnect(string partnerCode, string bankNo, string accNo, string private_key, string remoteIP)
        {
            string passPhrase = "Pas5pr@se";
            string saltValue = "s@1tValue";
            string hashAlgorithm = "SHA1";
            int passwordIterations = 2;
            string initVector = "@CSS@CSS@CSS@CSS";
            int keySize = 256;
            string dePartner_code = EncryptData.Decrypt(partnerCode, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
            string result_req = "";
            try
            {
                Random rnd = new Random();
                int code = rnd.Next(100000000, 999999999);

                RequestObjectVerify request = new RequestObjectVerify();
                request.RequestId = dePartner_code + "BK" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + code.ToString();
                request.RequestTime = DataGeneratorUtil.getVietNamDateTime("yyyy-MM-dd HH:mm:ss");
                request.PartnerCode = dePartner_code;
                request.Operation = 9001;
                request.BankNo = bankNo;
                request.AccNo = accNo;
                request.AccType = 0;


                string dataSign = request.RequestId + "|" + request.RequestTime + "|" + request.PartnerCode + "|" + request.Operation + "|" + request.BankNo + "|" + request.AccNo + "|" + request.AccType;
                request.Signature = RSASign.sign(dataSign, private_key);

                string json_request = System.Text.Json.JsonSerializer.Serialize(request, option);
                Console.WriteLine("Request:\n" + json_request + "\n");
                _logging.insertLogging(new LoggingRequest
                {
                    user_type = Consts.USER_TYPE_WEB_PARTNER,
                    is_call_api = true,
                    api_name = "Function checkVerifyConnect",
                    actions = "Verify connect BK",
                    application = "APP LOYALTY",
                    content = "Request:\n" + json_request + "\n",
                    functions = "APP LOYALTY",
                    is_login = false,
                    result_logging = "Call to BK",
                    user_created = "Anonymous",
                    IP = remoteIP
                });

                Task<string> response = HttpsRequestUtil.postToAddress(BKConsts.API_URL, json_request);
                result_req = response.Result;

                CheckVerifyConnect result = (CheckVerifyConnect)System.Text.Json.JsonSerializer.Deserialize(response.Result, typeof(CheckVerifyConnect));

                return result;
            }
            catch (Exception ex)
            {
                CheckVerifyConnect result = new CheckVerifyConnect();
                result.ResponseCode = 999;
                result.ResponseMessage = ex.Message + " :" + result_req;
                return result;
            }
        }
    }
}
