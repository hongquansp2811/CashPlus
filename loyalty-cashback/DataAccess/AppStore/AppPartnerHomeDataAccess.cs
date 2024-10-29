using System;
using System.Linq;
using System.Collections.Generic;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.PaymentGate;
using MailKit.Search;

namespace LOYALTY.DataAccess
{
    public class AppPartnerHomeDataAccess : IAppPartnerHome
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly BKTransaction _bkTransaction;
        public AppPartnerHomeDataAccess(LOYALTYContext context, ICommonFunction commonFunction, BKTransaction bkTransaction)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _bkTransaction = bkTransaction;
        }

        public APIResponse getHomeInfo(Guid partner_id, string username)
        {

            var partnerObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_NOT_FOUND");
            }

            var bonusConfigServiceTypeObj = _context.BonusPointConfigs.Where(x => x.service_type_id == partnerObj.service_type_id && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.active == true).FirstOrDefault();

            if (bonusConfigServiceTypeObj == null)
            {
                bonusConfigServiceTypeObj = _context.BonusPointConfigs.Where(x => x.service_type_id == null && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.active == true).FirstOrDefault();
            }

            string bonus_description = "";
            if (bonusConfigServiceTypeObj != null)
            {
                bonus_description = bonusConfigServiceTypeObj.description;
            }

            var dateNow = DateTime.Now;
            var settings = _context.Settingses.FirstOrDefault();

            long amount_balance = 0;
            var error_mess = new object();

            if (partnerObj.bk_partner_code != null && partnerObj.RSA_privateKey != null)
            {
 
                GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);

                amount_balance = balanceObj.Available;
                error_mess =  balanceObj;
            }

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;
            var dataResponse = (from p in _context.Partners
                                join u in _context.Users.Where(x => x.is_partner_admin == true) on p.id equals u.partner_id
                                join mu in _context.Users.Where(x => x.is_partner == true && x.username == username) on p.id equals mu.partner_id
                                join st in _context.OtherLists on p.status equals st.id into sts
                                from st in sts.DefaultIfEmpty()
                                join c in _context.PartnerContracts.Where(x => x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12) on p.id equals c.partner_id into cs
                                from c in cs.DefaultIfEmpty()
                                where p.id == partner_id
                                select new
                                {
                                    partner_id = p.id,
                                    partner_name = p.name,
                                    phone = p.phone,
                                    avatar = mu.avatar,
                                    email = mu.email,
                                    address = p.address,
                                    status = p.status,
                                    status_name = st != null ? st.name : "",
                                    share_code = u.share_code,
                                    share_code_link = Consts.LINK_SHARE + u.share_code,
                                    total_point = u.total_point,
                                    point_waiting = u.point_waiting,
                                    point_affiliate = u.point_affiliate,
                                    point_avaiable = u.point_avaiable,
                                    point_exchange_rate = pointExchangeRate,
                                    is_have_secret_key = mu.secret_key != null ? true : false,
                                    discount_rate = c != null ? c.discount_rate : 0,
                                    min_change_point = settings.change_point_estimate,
                                    max_change_point = settings.approve_change_point_min,
                                    point_value = settings.point_value,
                                    point_exchange = settings.point_exchange,
                                    add_point_policy = settings.add_point_policy,
                                    bonus_description = bonus_description,
                                    consumption_point_description = settings.partner_consumption_point_description,
                                    affiliate_point_description = settings.partner_affiliate_point_description,
                                    app_partner_spinner = settings.app_partner_spinner,
                                    amount_balance = amount_balance,
                                    is_manage_user = mu.is_partner_admin == true ? true : (mu.is_manage_user != null ? mu.is_manage_user : false),
                                    is_add_point_permission = mu.is_partner_admin == true ? true : (mu.is_add_point_permission != null ? mu.is_add_point_permission : false),
                                    is_change_point_permission = mu.is_partner_admin == true ? true : (mu.is_change_point_permission != null ? mu.is_change_point_permission : false),
                                    list_banners = _context.Banners.Where(x => x.start_date <= dateNow && x.end_date >= dateNow).ToList(),
                                    amount_limit = settings.amount_limit,
                                    bank_name = _context.Banks.Where(l => l.id == p.bk_bank_id).Select(p => p.name).FirstOrDefault(),
                                    bank_no = p.bk_bank_no,
                                    link_QR = p.link_QR,
                                    name_bank_own = p.bk_bank_owner,
                                    send_Notification = u.send_Notification,
                                    send_Popup = u.send_Popup,
                                    SMS_addPointSave = u.SMS_addPointSave,
                                    SMS_addPointUse = u.SMS_addPointUse,
                                    error_mess = error_mess                                  
                                }).FirstOrDefault();

            return new APIResponse(dataResponse);
        }

        public APIResponse generalInfo()
        {
            var settings = _context.Settingses.FirstOrDefault();

            var dataResponse = new {
                consumption_point_description = settings.partner_consumption_point_description,
                affiliate_point_description = settings.partner_affiliate_point_description,
                app_partner_spinner = settings.app_partner_spinner
            };

            return new APIResponse(dataResponse);
        }

        public APIResponse getNotiNotRead(Guid partner_id)
        {
            var totalNotification = (from p in _context.Notifications
                                     where p.user_id == null || (p.user_id != null && p.user_id == partner_id)
                                     select p.id).Count();


            var totalNotificationRead = (from p in _context.Notifications
                                         join cp in _context.UserNotifications on p.id equals cp.notification_id
                                         where (p.user_id == null || (p.user_id != null && p.user_id == partner_id)) && cp.user_id == partner_id
                                         select p.id).Count();

            var total_noti_not_read = totalNotification - totalNotificationRead;

            return new APIResponse(new
            {
                total_noti_not_read = total_noti_not_read
            });

        }

        public APIResponse getCustomerFakeBank(Guid partner_id)
        {
            var data = (from p in _context.CustomerFakeBanks
                        where p.user_id == partner_id
                        select p).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_HAVE_FAKE_BANK");
            }
            return new APIResponse(data);

        }
    }
}
