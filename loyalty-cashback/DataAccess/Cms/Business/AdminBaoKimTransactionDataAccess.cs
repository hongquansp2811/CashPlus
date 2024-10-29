using System;
using System.Linq;
using System.Text.Json;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.PaymentGate;

namespace LOYALTY.DataAccess
{
    public class AdminBaoKimTransactionDataAccess : IAdminBaoKimTransaction
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private static JsonSerializerOptions option;
        private static BKTransaction _bkTransaction;
        public AdminBaoKimTransactionDataAccess(LOYALTYContext context, ICommonFunction commonFunction, BKTransaction bkTransaction)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _bkTransaction = bkTransaction;
            option = new JsonSerializerOptions { WriteIndented = true };
        }

        public APIResponse getListBKTransaction(AccumulatePointOrderRequest request)
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

            var lstData = (from p in _context.BaoKimTransactions
                           join ord in _context.AccumulatePointOrders on p.accumulate_point_order_id equals ord.id into ords
                           from ord in ords.DefaultIfEmpty()
                           join s in _context.Partners on ord.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join st in _context.OtherLists on p.trans_status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.transaction_date descending
                           select new
                           {
                               id = p.id,
                               order_trans_no = ord.trans_no,
                               transaction_no = p.transaction_no,
                               bao_kim_transaction_id = p.bao_kim_transaction_id,
                               partner_id = ord.partner_id,
                               trans_date_origin = p.transaction_date,
                               trans_date = p.transaction_date != null ? _commonFunction.convertDateToStringFull(p.transaction_date) : "",
                               transaction_source = p.payment_type == "CP_TRANSFER_NTD" ? "CASHPLUS" : s.code,
                               transaction_source_origin = p.payment_type != "CP_TRANSFER_NTD" ? s.id : null,
                               amount = p.amount,
                               bank_receive_name = p.bank_receive_name,
                               bank_receive_owner = p.bank_receive_owner,
                               bank_receive_account = p.bank_receive_account,
                               payment_type = p.payment_type,
                               status = p.trans_status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.trans_no != null)
            {
                lstData = lstData.Where(x => x.order_trans_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()) || x.transaction_no.Trim().ToLower().Contains(request.trans_no.Trim().ToLower())
                || x.bao_kim_transaction_id.Trim().ToLower().Contains(request.trans_no.Trim().ToLower()));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.payment_type != null)
            {
                lstData = lstData.Where(x => x.payment_type == request.payment_type);
            }

            if (request.list_payment_type != null && request.list_payment_type.Count > 0)
            {
                lstData = lstData.Where(x => request.list_payment_type.Contains(x.payment_type) == true);
            }

            if (request.list_status != null && request.list_status.Count > 0)
            {
                lstData = lstData.Where(x => request.list_status.Contains(x.status) == true);
            }

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.transaction_source_origin == request.partner_id);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
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

        public APIResponse getListPartnerBK(AccumulatePointOrderRequest request)
        {
            if (request.from_date == null || request.from_date.Length == 0 || request.to_date == null || request.to_date.Length == 0)
            {
                return new APIResponse(400);
            }

            if (_commonFunction.convertDateToStringSort(DateTime.Now) == request.from_date || _commonFunction.convertDateToStringSort(DateTime.Now) == request.to_date)
            {
                return new APIResponse(400);
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

            var lstData = (from p in _context.Partners
                           select new
                           {
                               partner_id = p.id,
                               partner_code = p.code,
                               partner_name = p.name,
                               total_amount = _context.BaoKimTransactions.Where(x => x.transaction_date >= _commonFunction.convertStringSortToDate(request.from_date).Date
                               && x.transaction_date <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1)
                               && x.trans_status == 27
                               && x.partner_id == p.id).Sum(x => x.amount)
                           });

            if (request.partner_id != null)
            {
                lstData = lstData.Where(x => x.partner_id == request.partner_id);
            }

            lstData = lstData.Where(x => x.total_amount > 0);
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

        public APIResponse getDetailBKTransaction(Guid id)
        {
            var data = (from p in _context.BaoKimTransactions
                        join ord in _context.AccumulatePointOrders on p.accumulate_point_order_id equals ord.id into ords
                        from ord in ords.DefaultIfEmpty()
                        join s in _context.Partners on ord.partner_id equals s.id into ss
                        from s in ss.DefaultIfEmpty()
                        join st in _context.OtherLists on p.trans_status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            order_trans_no = ord.trans_no,
                            transaction_no = p.transaction_no,
                            bao_kim_transaction_id = p.bao_kim_transaction_id,
                            partner_id = ord.partner_id,
                            trans_date_origin = p.transaction_date,
                            trans_date = p.transaction_date != null ? _commonFunction.convertDateToStringFull(p.transaction_date) : "",
                            transaction_source = p.payment_type == "CP_TRANSFER_NTD" ? "CASHPLUS" : s.code,
                            amount = p.amount,
                            bank_receive_name = p.bank_receive_name,
                            bank_receive_owner = p.bank_receive_owner,
                            bank_receive_account = p.bank_receive_account,
                            payment_type = p.payment_type,
                            status = p.trans_status,
                            status_name = st != null ? st.name : ""
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse paymentCashPlus(AccumulatePointOrderRequest request)
        {
            if (request.from_date == null || request.from_date.Length == 0 || request.to_date == null || request.to_date.Length == 0)
            {
                return new APIResponse(400);
            }

            if (_commonFunction.convertDateToStringSort(DateTime.Now) == request.from_date || _commonFunction.convertDateToStringSort(DateTime.Now) == request.to_date)
            {
                return new APIResponse(400);
            }

            var settingObj = _context.Settingses.FirstOrDefault();

            if (settingObj == null || settingObj.point_exchange == null || settingObj.point_value == null)
            {
                return new APIResponse("ERROR_SETTINGS_NOT_CONFIG");
            }

            var sysBankObj = _context.Banks.Where(x => x.id == settingObj.sys_receive_bank_id).FirstOrDefault();
            if (sysBankObj == null)
            {
                return new APIResponse("ERROR_SYSTEM_BANK_NOT_EXISTS");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_EXISTS");
            }

            if (partnerObj.status != 15)
            {
                return new APIResponse("ERROR_PARTNER_ID_NOT_AVAIABLE");
            }

            if (partnerObj.bk_partner_code == null)
            {
                return new APIResponse("ERROR_PARTNER_BAOKIM_NOT_AVAIABLE");
            }

            var lstData = (from p in _context.Partners
                           where p.id == request.partner_id
                           select new
                           {
                               partner_id = p.id,
                               partner_code = p.code,
                               partner_name = p.name,
                               total_amount = _context.BaoKimTransactions.Where(x => x.transaction_date >= _commonFunction.convertStringSortToDate(request.from_date).Date
                               && x.transaction_date <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1)
                               && x.trans_status == 27
                               && x.partner_id == p.id).Sum(x => x.amount)
                           }).FirstOrDefault();

            var listTransaction = _context.BaoKimTransactions.Where(x => x.transaction_date >= _commonFunction.convertStringSortToDate(request.from_date).Date
            && x.transaction_date <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1)
                               && x.trans_status == 27
                               && x.partner_id == request.partner_id).ToList();


            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Chuyển khoản từ deposit merchant sang CashPlus CK CashPLus + Affiliate
                TransferResponseObj response2 = _bkTransaction.transferMoney("CASHPLUS", "970409", "9704060224009513", "Nguyen Van A", (decimal)lstData.total_amount, "Chuyen tien tai BK voi ma don hang " + request.trans_no, Consts.private_key);
                //TransferResponseObj response2 = _bkTransaction.transferMoney(partnerObj.bk_partner_code, sysBankObj.bank_code, settingObj.sys_receive_bank_no, settingObj.sys_receive_bank_owner, lstData.total_amount.ToString(), "Chuyen tien tai BK voi ma don hang " + request.trans_no);

                // Log giao dịch chuyển sang cho CashPlus
                BaoKimTransaction sysTrans = new BaoKimTransaction();
                sysTrans.id = Guid.NewGuid();
                sysTrans.payment_type = "MER_TRANSFER_SYS_1";
                sysTrans.bao_kim_transaction_id = response2.TransactionId;
                sysTrans.transaction_no = response2.ReferenceId;
                sysTrans.amount = lstData.total_amount;
                sysTrans.accumulate_point_order_id = request.id;
                sysTrans.partner_id = request.partner_id;
                sysTrans.customer_id = request.customer_id;
                sysTrans.transaction_description = "Chuyen tien tai BK voi ma don hang " + request.trans_no;

                sysTrans.bank_receive_name = sysBankObj.name;
                sysTrans.bank_receive_account = settingObj.sys_receive_bank_no;
                sysTrans.bank_receive_owner = settingObj.sys_receive_bank_owner;
                if (response2.ResponseCode == 200)
                {
                    sysTrans.trans_status = 25;
                    // Nếu thành công thì cập nhật các giao dịch
                    for (int i = 0; i < listTransaction.Count; i++)
                    {
                        listTransaction[i].trans_status = 25;
                    }

                    _context.SaveChanges();
                }
                else
                {
                    sysTrans.trans_status = 26;
                }
                sysTrans.transaction_date = DateTime.Now;
                sysTrans.trans_log = JsonSerializer.Serialize(response2, option);

                _context.BaoKimTransactions.Add(sysTrans);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(400);
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }

    }
}
