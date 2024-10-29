using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using System.Threading.Tasks;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.CloudMessaging;
using LOYALTY.PaymentGate;
using Org.BouncyCastle.Asn1.Ocsp;
using DocumentFormat.OpenXml.Office2016.Excel;

namespace LOYALTY.DataAccess
{
    public class AppAccumulatePointOrderDataAccess : IAppAccumulatePointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly FCMNotification _fCMNotification;
        private readonly BKTransaction _bkTransaction;
        private readonly IPartnerAccumulatePointOrder _partnerAccumulatePointOrder;
        public AppAccumulatePointOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction, FCMNotification fCMNotification, BKTransaction bkTransaction, IPartnerAccumulatePointOrder partnerAccumulatePointOrder)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _fCMNotification = fCMNotification;
            _bkTransaction = bkTransaction;
            _partnerAccumulatePointOrder = partnerAccumulatePointOrder;
        }

        public APIResponse getPartnerDetailByQR(Guid id)
        {
            var data = (from p in _context.Partners
                        join c in _context.PartnerContracts.Where(x => x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12) on p.id equals c.partner_id into cs
                        from c in cs.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            code = p.code,
                            name = p.name,
                            phone = p.phone,
                            address = p.address,
                            discount_rate = c != null ? c.discount_rate : 0
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getList(AccumulatePointOrderRequest request)
        {
            var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;

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

            var lstData = (from p in _context.AccumulatePointOrders
                           join s in _context.Partners on p.partner_id equals s.id
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                               date_created = p.date_created,
                               partner_name = s.name,
                               bill_amount = p.bill_amount,
                               payment_type = p.return_type,
                               point_exchange = p.point_customer,
                               amount_exchange = Math.Round((decimal)p.point_customer * pointExchangeRate),
                               description = p.description,
                               approve_user = p.approve_user,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.payment_type != null && request.payment_type.Length > 0)
            {
                lstData = lstData.Where(x => x.payment_type == request.payment_type);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.date_created <= _commonFunction.convertStringSortToDate(request.to_date).AddHours(23).AddMinutes(59));
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
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;

            var data = (from p in _context.AccumulatePointOrders
                        join s in _context.Partners on p.partner_id equals s.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        join com in _context.AccumulatePointOrderComplains on p.id equals com.accumulate_order_id into coms
                        from com in coms.DefaultIfEmpty()
                        join ra in _context.AccumulatePointOrderRatings on p.id equals ra.accumulate_point_order_id into ras
            from ra in ras.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            trans_no = p.trans_no,
                            trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                            partner_name = s.name,
                            partner_id = p.partner_id,
                            partner_address = s.address,
                            partner_phone = s.phone,
                            bill_amount = p.bill_amount,
                            payment_type = p.return_type,
                            point_exchange = p.point_customer,
                            amount_exchange = Math.Round((decimal)p.point_customer * pointExchangeRate),
                            description = p.description,
                            approve_user = p.user_created,
                            status = p.status,
                            is_complain = com != null ? true : false,
                            is_review = ra != null ? true : false,
                            status_name = st != null ? st.name : "",
                            amount_partner = Math.Round((decimal)p.point_partner * pointExchangeRate),
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }


        public APIResponse create(AccumulatePointOrderRequest request, string username)
        {
            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.bill_amount == null)
            {
                return new APIResponse("ERROR_BILL_AMOUNT_MISSING");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_EXISTS");
            }

            var userPartner = _context.Users.Where(x => x.is_partner == true && x.is_partner_admin == true && x.partner_id == request.partner_id).FirstOrDefault();

            if (userPartner == null)
            {
                return new APIResponse("ERROR_USER_PARTNER_NOT_EXISTS");
            }

            //if (partnerObj.status != 15)
            //{
            //    userPartner.is_violation = true;
            //    _context.SaveChanges();
            //    return new APIResponse("ERROR_PARTNER_ID_NOT_AVAIABLE");
            //}

            var dateNow = DateTime.Now;

            var contractObj = _context.PartnerContracts.Where(x => x.status == 12 && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id).FirstOrDefault();
            if (contractObj == null)
            {
                return new APIResponse("ERROR_CONTRACT_NOT_EXISTS");
            }

            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null)
            {
                return new APIResponse("ERROR_SETTINGS_NOT_CONFIG");
            }

            long amount_balance = 0;

            if (partnerObj.bk_partner_code != null)
            {
                GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

                amount_balance = balanceObj.Available;
            }

            decimal pointExchangeRate = Math.Round(((decimal)settingObj.point_exchange / (decimal)settingObj.point_value), 2);

            decimal total_money = Math.Round((decimal)request.bill_amount * (decimal)contractObj.discount_rate / 100);

            //if (amount_balance < total_money)
            //{
            //    return new APIResponse("ERROR_PARTNER_ENOUGH_AMOUNT");
            //}

            var transaction = _context.Database.BeginTransaction();
            Guid orderId = Guid.NewGuid();

            try
            {
                var maxCodeObject = _context.AccumulatePointOrders.Where(x => x.trans_no != null && x.trans_no.Contains("TD")).OrderByDescending(x => x.trans_no).FirstOrDefault();
                string codeOrder = "";
                if (maxCodeObject == null)
                {
                    codeOrder = "TD0000000000001";
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
                    codeOrder = "TD" + orderString.PadLeft(number, pad);
                }

                var data = new AccumulatePointOrder();
                data.id = orderId;
                data.customer_id = request.customer_id;
                data.partner_id = request.partner_id;
                data.trans_no = codeOrder;
                data.description = request.description;
                data.bill_amount = request.bill_amount;
                data.payment_type = "Cash";
                data.return_type = "Point";
                data.point_exchange = 0;
                data.point_customer = 0;
                data.point_partner = 0;
                data.point_system = 0;
                data.status = 4;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.AccumulatePointOrders.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_ADD_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(new
            {
                order_id = orderId,
                amount = request.bill_amount
            });
        }

        public APIResponse updateOrder(AccumulatePointOrderRequest request)
        {
            if (request.order_id == null)
            {
                return new APIResponse("ERROR_ORDER_ID_MISSING");
            }

            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            var data = _context.AccumulatePointOrders.Where(x => x.id == request.order_id).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_ORDER_ID_INCORRECT");
            }

            if (data.customer_id != null)
            {
                return new APIResponse("ERROR_ORDER_ID_HAVE_CUSTOMER");
            }

            try
            {
                data.customer_id = request.customer_id;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public async Task<APIResponse> createPaymentLink(AccumulatePointOrderRequest request)
        {
            if (request.order_id == null)
            {
                return new APIResponse("ERROR_ORDER_ID_MISSING");
            }

            var orderObj = _context.AccumulatePointOrders.Where(x => x.id == request.order_id).FirstOrDefault();

            string return_url = "";
            if (orderObj != null)
            {
                return_url = await _bkTransaction.createPaymentLink((Guid)request.order_id);
                orderObj.payment_type = "BaoKim";
                _context.SaveChanges();


            }
            var return_url_split = return_url.Split("+/");
            var check = return_url_split[0];

            int status = 200;

            if (check.Contains("Error")) status = 500;

            return new APIResponse(status, new { payment_link = return_url_split[1] });
        }

        public APIResponse createRating(AccumulatePointOrderRatingRequest request, string username)
        {
            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.rating == null)
            {
                return new APIResponse("ERROR_RATING_MISSING");
            }

            if (request.accumulate_point_order_id == null)
            {
                return new APIResponse("ERROR_ACCUMULATE_POINT_ORDER_ID_MISSING");
            }

            var dataSame = _context.AccumulatePointOrderRatings.Where(x => x.accumulate_point_order_id == request.accumulate_point_order_id).FirstOrDefault();

            if (dataSame != null)
            {
                return new APIResponse("ERROR_ORDER_EXISTS_RATING");
            }

            var orderObj = _context.AccumulatePointOrders.Where(x => x.id == request.accumulate_point_order_id).FirstOrDefault();
            var customerObj = _context.Customers.Where(x => x.id == request.customer_id).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();
            var userPartnerObj = _context.Users.Where(x => x.partner_id == request.partner_id && x.is_partner_admin == true).FirstOrDefault();

            var ratingName = _context.RatingConfigs.Where(x => x.rating == request.rating.ToString()).Select(x => x.description).FirstOrDefault();

            if (ratingName == null)
            {
                if (request.rating == 1)
                {
                    ratingName = "Rất tệ";
                }
                else if (request.rating == 2)
                {
                    ratingName = "Tệ";
                }
                else if (request.rating == 3)
                {
                    ratingName = "Bình thường";
                }
                else if (request.rating == 4)
                {
                    ratingName = "Tốt";
                }
                else if (request.rating == 5)
                {
                    ratingName = "Rất tốt";
                }
            }
            var transaction = _context.Database.BeginTransaction();

            try
            {
                // Khởi tạo Rating
                var newRating = new AccumulatePointOrderRating();
                newRating.id = Guid.NewGuid();
                newRating.customer_id = request.customer_id;
                newRating.partner_id = request.partner_id;
                newRating.rating = request.rating;
                newRating.content = request.content;
                newRating.accumulate_point_order_id = request.accumulate_point_order_id;
                newRating.rating_name = ratingName;
                newRating.date_created = DateTime.Now;
                newRating.date_updated = DateTime.Now;
                newRating.user_created = username;
                newRating.user_updated = username;

                _context.AccumulatePointOrderRatings.Add(newRating);

                _context.SaveChanges();

                // Cập nhật Rating cửa hàng
                if (partnerObj.total_rating == null)
                {
                    partnerObj.total_rating = 0;
                }

                if (partnerObj.rating == null)
                {
                    partnerObj.rating = 0;
                }

                partnerObj.rating = Math.Round((((decimal)partnerObj.rating * (decimal)partnerObj.total_rating) + (decimal)request.rating) * 100 / ((decimal)partnerObj.total_rating + 1)) / 100;
                partnerObj.total_rating = partnerObj.total_rating + 1;

                _context.SaveChanges();

                // Gửi FCM cho Shop
                if (userPartnerObj.device_id != null)
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Đánh giá khách hàng";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = userPartnerObj.partner_id;
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    newNoti1.content = "Khách hàng " + customerObj.phone + " vừa thực hiện đánh giá giao dịch với Mã giao dịch " + orderObj.trans_no + " vào lúc " + _commonFunction.convertDateToStringFull(orderObj.date_created);
                    newNoti1.system_type = "INFO";
                    newNoti1.reference_id = null;

                    _context.Notifications.Add(newNoti1);
                    _context.SaveChanges();

                    _fCMNotification.SendNotification(userPartnerObj.device_id,
                        "INFO",
                        "Đánh giá khách hàng",
                        "Khách hàng " + customerObj.phone + " vừa thực hiện đánh giá giao dịch với Mã giao dịch " + orderObj.trans_no + " vào lúc " + _commonFunction.convertDateToStringFull(orderObj.date_created),
                        null);
                }
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                transaction.Dispose();
                return new APIResponse("ERROR_ADD_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }

        public APIResponse QR(AccumulatePointOrderRequest request)
        {
            var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = Math.Round((pointExchange / pointValue), 2);


            var data = (from p in _context.AccumulatePointOrders
                        join s in _context.Partners on p.partner_id equals s.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        join com in _context.AccumulatePointOrderComplains on p.id equals com.accumulate_order_id into coms
                        from com in coms.DefaultIfEmpty()
                        join ra in _context.AccumulatePointOrderRatings on p.id equals ra.accumulate_point_order_id into ras
                        from ra in ras.DefaultIfEmpty()
                        where p.id == request.id
                        select new
                        {
                            id = p.id,
                            trans_no = p.trans_no,
                            trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                            partner_name = s.name,
                            partner_id = p.partner_id,
                            partner_address = s.address,
                            partner_phone = s.phone,
                            bill_amount = p.bill_amount,
                            payment_type = p.return_type,
                            point_exchange = p.point_customer,
                            amount_exchange = Math.Round((decimal)p.point_customer * pointExchangeRate),
                            description = p.description,
                            approve_user = p.approve_user,
                            status = p.status,
                            is_complain = com != null ? true : false,
                            is_review = ra != null ? true : false,
                            status_name = st != null ? st.name : "",
                            amount_partner = Math.Round((decimal)p.point_partner * pointExchangeRate),
                        }).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            var dataEx = _context.AccumulatePointOrders.Where(p => p.id == request.id).FirstOrDefault();

            dataEx.customer_id = request.customer_id;
            _context.SaveChanges();

            return new APIResponse(data);
        }
        

        public APIResponse getByTransNo(string? trans_no){
             var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = Math.Round((pointExchange / pointValue), 2);
            
            var data = (from p in _context.AccumulatePointOrders
                        join s in _context.Partners on p.partner_id equals s.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        join com in _context.AccumulatePointOrderComplains on p.id equals com.accumulate_order_id into coms
                        from com in coms.DefaultIfEmpty()
                        join ra in _context.AccumulatePointOrderRatings on p.id equals ra.accumulate_point_order_id into ras
                        from ra in ras.DefaultIfEmpty()
                        where p.trans_no == trans_no
                        select new
                        {
                            id = p.id,
                            trans_no = p.trans_no,
                            trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                            partner_name = s.name,
                            partner_id = p.partner_id,
                            partner_address = s.address,
                            partner_phone = s.phone,
                            bill_amount = p.bill_amount,
                            payment_type = p.return_type,
                            point_exchange = p.point_customer,
                            amount_exchange = Math.Round((decimal)p.point_customer * pointExchangeRate),
                            description = p.description,
                            approve_user = p.approve_user,
                            status = p.status,
                            is_complain = com != null ? true : false,
                            is_review = ra != null ? true : false,
                            status_name = st != null ? st.name : "",
                            amount_partner = Math.Round((decimal)p.point_partner * pointExchangeRate),
                        }).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            return new APIResponse(data);
        }

    }
}
