using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class BankAccountDataAccess : IBankAccount
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public BankAccountDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(BankAccountRequest request)
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
            //.Take(request.page_size).Skip(skipElements)
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.BankAccounts
                           join b in _context.Banks on p.bank_id equals b.id into bs
                           from b in bs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               bank_id = p.bank_id,
                               bank_name = b.name,
                               bank_no = p.bank_no,
                               bank_owner = p.bank_owner,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.bank_no != null && request.bank_no.Length > 0)
            {
                lstData = lstData.Where(x => x.bank_no.Trim().ToLower().Contains(request.bank_no.Trim().ToLower()) || x.bank_owner.Trim().ToLower().Contains(request.bank_no.Trim().ToLower()));
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
            var data = (from p in _context.BankAccounts
                        join b in _context.Banks on p.bank_id equals b.id into bs
                        from b in bs.DefaultIfEmpty()
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            bank_id = p.bank_id,
                            bank_name = b.name,
                            bank_no = p.bank_no,
                            bank_owner = p.bank_owner,
                            status = p.status,
                            status_name = st != null ? st.name : ""
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(BankAccountRequest request, string username)
        {
            if (request.bank_id == null)
            {
                return new APIResponse("ERROR_BANK_ID_MISSING");
            }

            if (request.bank_no == null)
            {
                return new APIResponse("ERROR_BANK_NO_MISSING");
            }

            if (request.bank_owner == null)
            {
                return new APIResponse("ERROR_BANK_OWNER_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            var dataSameCode = _context.BankAccounts.Where(x => x.bank_no.ToLower().Trim() == request.bank_no.ToLower().Trim() && x.bank_id == request.bank_id).FirstOrDefault();

            if (dataSameCode != null)
            {
                return new APIResponse("ERROR_BANK_ACCOUNT_SAME_NO");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Tạo cửa hàng
                var data = new BankAccount();
                data.id = Guid.NewGuid();
                data.bank_id = request.bank_id;
                data.bank_no = request.bank_no;
                data.bank_owner = request.bank_owner;
                data.status = request.status;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.BankAccounts.Add(data);
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

        public APIResponse update(BankAccountRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.BankAccounts.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.bank_id == null)
            {
                return new APIResponse("ERROR_BANK_ID_MISSING");
            }

            if (request.bank_no == null)
            {
                return new APIResponse("ERROR_BANK_NO_MISSING");
            }

            if (request.bank_owner == null)
            {
                return new APIResponse("ERROR_BANK_OWNER_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            try
            {
                data.bank_id = request.bank_id;
                data.bank_no = request.bank_no;
                data.bank_owner = request.bank_owner;
                data.status = request.status;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest request)
        {
            var data = _context.BankAccounts.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.BankAccounts.Remove(data);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_DELETE_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }
    }
}
