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
    public class SMSHistoryDatatAccess : ISMSHistory
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public SMSHistoryDatatAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse create(SMSHistoryReq request)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var data = new SMSHistory();
                data.id = Guid.NewGuid();

                data.SMSID = request.SMSID;
                data.CodeResult = request.CodeResult;
                data.ErrorMessage =  request.ErrorMessage;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.SMSHistories.Add(data);
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
            return new APIResponse(200);
        }

    }
}
