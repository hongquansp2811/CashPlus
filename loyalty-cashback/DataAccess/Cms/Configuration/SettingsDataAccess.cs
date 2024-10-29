using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class SettingsDataAccess : ISettings
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public SettingsDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getDetail()
        {
            var data = (from p in _context.Settingses
                        select new
                        {
                            id = p.id,
                            point_value = p.point_value,
                            point_exchange = p.point_exchange,
                            policy = p.policy,
                            consumption_point_description = p.consumption_point_description,
                            affiliate_point_description = p.affiliate_point_description,
                            app_customer_banner_image = p.app_customer_banner_image,
                            app_customer_spinner_image = p.app_customer_spinner_image,
                            app_customer_wallet_image = p.app_customer_wallet_image,
                            app_customer_qr_image = p.app_customer_qr_image,
                            app_customer_affiliate_image = p.app_customer_affiliate_image,
                            app_customer_gift_image = p.app_customer_gift_image,
                            app_partner_spinner = p.app_partner_spinner,
                            add_point_policy = p.add_point_policy,
                            sms_point_config = p.sms_point_config,
                            change_point_estimate = p.change_point_estimate,
                            approve_change_point_min = p.approve_change_point_min,
                            total_allow_cash = p.total_allow_cash,
                            cash_condition_value = p.cash_condition_value,
                            partner_consumption_point_description = p.partner_consumption_point_description,
                            partner_affiliate_point_description = p.partner_affiliate_point_description,
                            app_partner_wallet_image = p.app_partner_wallet_image,
                            app_partner_qr_image = p.app_partner_qr_image,
                            app_partner_employee_image = p.app_partner_employee_image,
                            app_partner_transaction_image = p.app_partner_transaction_image,
                            sys_receive_bank_id = p.sys_receive_bank_id,
                            sys_receive_bank_name = p.sys_receive_bank_name,
                            sys_receive_bank_no = p.sys_receive_bank_no,
                            sys_receive_bank_owner = p.sys_receive_bank_owner,
                            eligible = p.eligible,
                            unconditional = p.unconditional,
                            name_Company = p.name_Company,
                            address_Company = p.address_Company,
                            DKKD = p.DKKD,
                            phone_Company = p.phone_Company,
                            Email_Company = p.Email_Company,
                            point_use = p.point_use,
                            point_save = p.point_save,
                            send_time = p.send_time,
                            collection_fee = p.collection_fee,
                            expense_fee = p.expense_fee,
                            payment_limit = p.payment_limit,
                            amount_limit = p.amount_limit
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse update(SettingsRequest request, string username)
        {
           
            var data = _context.Settingses.FirstOrDefault();
            bool isNew = false;
            if (data == null)
            {
                isNew = true;
                data = new Settings();
                data.id = Guid.NewGuid();
            }

            try
            {
                var newHistory = new ConfigHistory();
                newHistory.id = Guid.NewGuid();

                if (request.point_value != null)
                {
                    data.point_value = request.point_value;
                    data.point_exchange = request.point_exchange;
                    newHistory.config_type = "CONFIG_POINT";
                }

                if (request.policy != null)
                {
                    data.policy = request.policy;
                    data.add_point_policy = request.add_point_policy;
                    newHistory.config_type = "CONFIG_POLICY";
                }

                if (request.sms_point_config != null)
                {
                    data.sms_point_config = request.sms_point_config;
                    newHistory.config_type = "CONFIG_SMS";
                }

                if (request.change_point_estimate != null)
                {
                    data.change_point_estimate = request.change_point_estimate;
                    data.approve_change_point_min = request.approve_change_point_min;
                    newHistory.config_type = "CONFIG_CHANGE_POINT_ESTIMATE";
                }

                if (request.consumption_point_description != null)
                {
                    data.consumption_point_description = request.consumption_point_description;
                    data.affiliate_point_description = request.affiliate_point_description;
                    data.app_customer_banner_image = request.app_customer_banner_image;
                    data.app_customer_spinner_image = request.app_customer_spinner_image;
                    data.app_customer_wallet_image = request.app_customer_wallet_image;
                    data.app_customer_qr_image = request.app_customer_qr_image;
                    data.app_customer_affiliate_image = request.app_customer_affiliate_image;
                    data.app_customer_gift_image = request.app_customer_gift_image;
                    data.app_partner_spinner = request.app_partner_spinner;
                    data.partner_consumption_point_description = request.partner_consumption_point_description;
                    data.partner_affiliate_point_description = request.partner_affiliate_point_description;
                    data.app_partner_qr_image = request.app_partner_qr_image;
                    data.app_partner_wallet_image = request.app_partner_wallet_image;
                    data.app_partner_transaction_image = request.app_partner_transaction_image;
                    data.app_partner_employee_image = request.app_partner_employee_image;
                    newHistory.config_type = "CONFIG_APP_HOME";
                    data.eligible = request.eligible;
                    data.unconditional = request.unconditional;
                }

                if (request.cash_condition_value != null)
                {
                    data.cash_condition_value = request.cash_condition_value;
                    data.total_allow_cash = request.total_allow_cash;
                    newHistory.config_type = "CONFIG_CASH_RETURN";
                }


                if (request.sys_receive_bank_id != null)
                {
                    data.sys_receive_bank_id = request.sys_receive_bank_id;
                    data.sys_receive_bank_name = request.sys_receive_bank_name;
                    data.sys_receive_bank_owner = request.sys_receive_bank_owner;
                    data.sys_receive_bank_no = request.sys_receive_bank_no;
                    newHistory.config_type = "CONFIG_SYS_BANK_ID";
                }
                if(request.name_Company != null)
                {
                    data.name_Company = request.name_Company;
                    data.address_Company = request.address_Company;
                    data.DKKD = request.DKKD;
                    data.phone_Company = request.phone_Company;
                    data.Email_Company = request.Email_Company;
                    newHistory.config_type = "CONFIG_COMPANY_INFO";
                }

                if(request.point_use != null)
                {
                    data.point_use = request.point_use;
                    data.point_save = request.point_save;
                    data.send_time = request.send_time;
                    newHistory.config_type = " SMS_Brandname";
                }

                if(request.collection_fee != null )
                {
                    data.collection_fee = request.collection_fee;
                    data.expense_fee = request.expense_fee;

                    newHistory.config_type = "RevenueAndExpenditureCosts";

                }

                if (request.payment_limit != null)
                {
                    data.payment_limit = request.payment_limit;
                    newHistory.config_type = "Payment_Limit";
                }

                if(request.amount_limit != null){
                    data.amount_limit = request.amount_limit;
                    newHistory.config_type = "Amount_limit";
                }
                newHistory.type = 1;
                newHistory.date_created = DateTime.Now;
                newHistory.user_created = username;
                newHistory.data_logging = JsonConvert.SerializeObject(request);
                if (isNew == true)
                {
                    _context.Settingses.Add(data);
                }

                _context.ConfigHistorys.Add(newHistory);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse getListHistory(ConfigHistoryRequest request)
        {
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.ConfigHistorys
                           where p.config_type == request.config_type
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               user_created = p.user_created,
                               date_created = _commonFunction.convertDateToStringFull(p.date_created),
                               data_logging = p.data_logging
                           });

            return new APIResponse(lstData);
        }

        public APIResponse getListCustomerRankConfig()
        {
            var lstData = _context.CustomerRankConfigs.ToList();

            return new APIResponse(lstData);
        }

        public APIResponse getListRatingConfig()
        {
            var lstData = _context.RatingConfigs.ToList();

            return new APIResponse(lstData);
        }

        public APIResponse getListExchangePointPackConfig()
        {
            var lstData = _context.ExchangePointPackConfigs.ToList();

            return new APIResponse(lstData);
        }

        public APIResponse updateCustomerRankConfig(CustomerRankConfigRequest request, string username)
        {
            try
            {
                var lstDeletes = _context.CustomerRankConfigs.ToList();
                _context.CustomerRankConfigs.RemoveRange(lstDeletes);

                if (request.list_items != null && request.list_items.Count > 0)
                {
                    for (int i = 0; i < request.list_items.Count; i++)
                    {
                        var item = new CustomerRankConfig();
                        item.id = Guid.NewGuid();
                        item.customer_rank_id = request.list_items[i].customer_rank_id;
                        item.customer_rank_name = request.list_items[i].customer_rank_name;
                        item.condition_upgrade = request.list_items[i].condition_upgrade;
                        _context.CustomerRankConfigs.Add(item);
                    }
                }

                _context.SaveChanges();

                var newHistory = new ConfigHistory();
                newHistory.id = Guid.NewGuid();
                newHistory.config_type = "CONFIG_CUSTOMER_RANK";
                newHistory.date_created = DateTime.Now;
                newHistory.user_created = username;
                newHistory.data_logging = JsonConvert.SerializeObject(request);
                _context.ConfigHistorys.Add(newHistory);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse updateExchangePointPackConfig(ExchangePointPackConfigRequest request, string username)
        {
            try
            {
                var lstDeletes = _context.ExchangePointPackConfigs.ToList();
                _context.ExchangePointPackConfigs.RemoveRange(lstDeletes);

                if (request.list_items != null && request.list_items.Count > 0)
                {
                    for (int i = 0; i < request.list_items.Count; i++)
                    {
                        var item = new ExchangePointPackConfig();
                        item.id = Guid.NewGuid();
                        item.point_exchange = request.list_items[i].point_exchange;
                        item.value_exchange = request.list_items[i].value_exchange;
                        _context.ExchangePointPackConfigs.Add(item);
                    }
                }

                _context.SaveChanges();

                var newHistory = new ConfigHistory();
                newHistory.id = Guid.NewGuid();
                newHistory.config_type = "CONFIG_EXCHANGE_POINT_PACK";
                newHistory.date_created = DateTime.Now;
                newHistory.user_created = username;
                newHistory.data_logging = JsonConvert.SerializeObject(request);
                _context.ConfigHistorys.Add(newHistory);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse updateRatingConfig(RatingConfigRequest request, string username)
        {
            try
            {
                var lstDeletes = _context.RatingConfigs.ToList();
                _context.RatingConfigs.RemoveRange(lstDeletes);

                if (request.list_items != null && request.list_items.Count > 0)
                {
                    for (int i = 0; i < request.list_items.Count; i++)
                    {
                        var item = new RatingConfig();
                        item.id = Guid.NewGuid();
                        item.rating = request.list_items[i].rating;
                        item.description = request.list_items[i].description;
                        _context.RatingConfigs.Add(item);
                    }
                }

                _context.SaveChanges();

                var newHistory = new ConfigHistory();
                newHistory.id = Guid.NewGuid();
                newHistory.config_type = "CONFIG_RATING";
                newHistory.date_created = DateTime.Now;
                newHistory.user_created = username;
                newHistory.data_logging = JsonConvert.SerializeObject(request);
                _context.ConfigHistorys.Add(newHistory);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }
    }
}
