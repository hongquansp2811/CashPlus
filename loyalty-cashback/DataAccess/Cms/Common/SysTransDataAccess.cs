using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using System.Threading.Tasks;

namespace LOYALTY.DataAccess
{
    public class SysTransDataAccess
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public SysTransDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            this._commonFunction = commonFunction;
        }

        public Task insertSystemTrans(DateTime trans_date, string trans_type, string type, decimal amount, Boolean is_balance)
        {
            int years = trans_date.Year;
            int quarter = (int)Math.Ceiling((decimal)trans_date.Month / 3);
            int month = trans_date.Month;
            int day = trans_date.Day;

            try
            {
                // Xử lý thêm số dư 
                var dataDetail = new SysAmountHistory();
                dataDetail.years = years;
                dataDetail.quarter = quarter;
                dataDetail.month = month;
                dataDetail.day = day;
                dataDetail.trans_date = trans_date;
                dataDetail.type = type;
                dataDetail.trans_type = trans_type;
                dataDetail.amount = amount;
                dataDetail.is_balance = is_balance;
                _context.SysAmountHistorys.Add(dataDetail);

                // Xử lý số tổng
                var dataInSummary = _context.SysAmountSummarys.Where(x => x.years == years && x.month == month && x.days == day).FirstOrDefault();
                if (dataInSummary == null)
                {
                    var dataSummary = new SysAmountSummary();
                    dataSummary.years = years;
                    dataSummary.quarter = quarter;
                    dataSummary.month = month;
                    dataSummary.days = day;

                    // Tìm số dư đầu kỳ
                    var dataMaxDay = (from p in _context.SysAmountSummarys
                                      orderby p.years descending, p.month descending, p.days descending
                                      select p
                                      ).FirstOrDefault();

                    if (type == "PUSH")
                    {
                        dataSummary.open_balance = dataMaxDay != null ? dataMaxDay.close_balance : 0;
                        dataSummary.push_balance = amount;
                        dataSummary.pull_balance = 0;
                        dataSummary.close_balance = dataSummary.open_balance - amount;
                    } else
                    {
                        dataSummary.open_balance = dataMaxDay != null ? dataMaxDay.close_balance : 0;
                        dataSummary.push_balance = 0;
                        dataSummary.pull_balance = amount;
                        dataSummary.close_balance = dataSummary.open_balance + amount;
                    }

                    _context.SysAmountSummarys.Add(dataSummary);
                } else
                {
                    if (type == "PUSH")
                    {
                        dataInSummary.push_balance += amount;
                        dataInSummary.close_balance = dataInSummary.open_balance + dataInSummary.pull_balance - dataInSummary.push_balance;
                    }
                    else
                    {
                        dataInSummary.pull_balance += amount;
                        dataInSummary.close_balance = dataInSummary.open_balance + dataInSummary.pull_balance - dataInSummary.push_balance;
                    }
                }

                _context.SaveChanges();
            } catch (Exception ex)
            {
                return Task.CompletedTask;
            }
    
            return Task.CompletedTask;
        }
    }
}
