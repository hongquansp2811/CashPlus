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
    public class NotiConfigDataAccess : INotiConfig
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public NotiConfigDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getDetail()
        {
            var data = (from p in _context.NotiConfigs
                        select new
                        {
                            id = p.id,
                            Payment_NotEnRefund = p.Payment_NotEnRefund,
                            Payment_Refund = p.Payment_Refund,
                            Payment_CheckSurplus = p.Payment_CheckSurplus,
                            Payment_SMSPointSave = p.Payment_SMSPointSave,
                            Payment_SMSPointUse = p.Payment_SMSPointUse,
                            ChangePoint_Add = p.ChangePoint_Add,
                            ChangePoint_Acp = p.ChangePoint_Acp,
                            ChangePoint_De = p.ChangePoint_De,
                            MC_Payment_Refund = p.MC_Payment_Refund,
                            MC_Payment_CheckSurplus = p.MC_Payment_CheckSurplus,
                            MC_Payment_SMSPointSave = p.MC_Payment_SMSPointSave,
                            MC_Payment_SMSPointUse = p.MC_Payment_SMSPointUse,
                            MC_Payment_Rating = p.MC_Payment_Rating,
                            Product_Acp = p.Product_Acp,
                            Product_De = p.Product_De,
                            MC_ChangePoint_Add = p.MC_ChangePoint_Add,
                            MC_ChangePoint_Acp = p.MC_ChangePoint_Acp,
                            MC_ChangePoint_De = p.MC_ChangePoint_De,
                            MC_amount_bill = p.MC_amount_bill,
                            MC_Payment_RefundFail = p.MC_Payment_RefundFail,
                            Payment_RefundFail = p.Payment_RefundFail
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse update(NotiConfigReq request, string username)
        {

            var data = _context.NotiConfigs.FirstOrDefault();
            bool isNew = false;
            if (data == null)
            {
                isNew = true;
                data = new NotiConfig();
                data.id = Guid.NewGuid();
            }

            try
            {
                var newHistory = new ConfigHistory();
                newHistory.id = Guid.NewGuid();

                if (request.Payment_NotEnRefund != null)
                {
                    data.Payment_NotEnRefund = request.Payment_NotEnRefund;
                    data.Payment_RefundFail = request.Payment_RefundFail;
                    data.Payment_Refund = request.Payment_Refund;
                    data.Payment_CheckSurplus = request.Payment_CheckSurplus;
                    data.Payment_SMSPointSave = request.Payment_SMSPointSave;
                    data.Payment_SMSPointUse = request.Payment_SMSPointUse;
                    data.ChangePoint_Add = request.ChangePoint_Add;
                    data.ChangePoint_Acp = request.ChangePoint_Acp;
                    data.ChangePoint_De = request.ChangePoint_De;
                    newHistory.config_type = "NOTI_APP_CASHPLUS";
                }

                if (request.MC_Payment_Refund != null)
                {
                    data.MC_Payment_RefundFail = request.MC_Payment_RefundFail;
                    data.MC_Payment_Refund = request.MC_Payment_Refund;
                    data.MC_Payment_CheckSurplus = request.MC_Payment_CheckSurplus;
                    data.MC_Payment_SMSPointSave = request.MC_Payment_SMSPointSave;
                    data.MC_Payment_SMSPointUse = request.MC_Payment_SMSPointUse;
                    data.MC_Payment_Rating = request.MC_Payment_Rating;
                    data.Product_Acp = request.Product_Acp;
                    data.Product_De = request.Product_De;
                    data.MC_ChangePoint_Add = request.MC_ChangePoint_Add;
                    data.MC_ChangePoint_Acp = request.MC_ChangePoint_Acp;
                    data.MC_ChangePoint_De = request.MC_ChangePoint_De;
                    data.MC_amount_bill = request.MC_amount_bill;
                    newHistory.config_type = "NOTI_APP_CASHPLUS_MERCHANT";
                }

                newHistory.date_created = DateTime.Now;
                newHistory.user_created = username;
                newHistory.data_logging = JsonConvert.SerializeObject(request);
                newHistory.type = 2;
                if (isNew == true)
                {
                    _context.NotiConfigs.Add(data);
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
                           where p.config_type == request.config_type && p.type == 2
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
    }
}
