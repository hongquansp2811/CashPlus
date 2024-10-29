using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class BankDataAccess : IBank
    {
        private readonly LOYALTYContext _context;
        public BankDataAccess(LOYALTYContext context) 
        {
            this._context = context;
        }

        public APIResponse getList(BankRequest request)
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
            var lstBank = _context.Banks.OrderByDescending(x => x.date_created).ToList();

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstBank = lstBank.Where(x => x.name.Trim().ToLower().Contains(request.name.Trim().ToLower())).ToList();
            }

            // Đếm số lượng
            int countElements = lstBank.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstBank.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(int id)
        {
            var bank = _context.Banks.Where(x => x.id == id).FirstOrDefault();
            if (bank == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(bank);
        }

        public APIResponse create(BankRequest request, string username)
        {

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }
            // Check trùng tên
            var dataBankName = _context.Banks.Where(x => x.name == request.name).FirstOrDefault();
            
            if (dataBankName != null)
            {
                return new APIResponse("ERROR_NAME_EXISTS");
            }

            if (request.avatar == null)
            {
                return new APIResponse("ERROR_AVATAR_MISSING");
            }
            try
            {
                var data = new Bank();
                data.name = request.name;
                data.avatar = request.avatar;
                data.background = request.background;
                data.description = request.description;
                data.active = request.active != null ? request.active : true;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.Banks.Add(data);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }
         
            return new APIResponse(200);
        }

        public APIResponse update(BankRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse(Messages.ERROR_ID_MISSING);
            }
            var bank = _context.Banks.Where(x => x.id == request.id).FirstOrDefault();
            if (bank == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.name != null && request.name.Length > 0)
            {
                bank.name = request.name;
            }

            if (request.avatar != null && request.avatar.Length > 0)
            {
                bank.avatar = request.avatar;
            }

            if (request.description != null && request.description.Length > 0)
            {
                bank.description = request.description;
            }

            try
            {
                bank.background = request.background;
                bank.active = request.active != null ? request.active : true;
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteRequest req)
        {
            var bank = _context.Banks.Where(x => x.id == req.id).FirstOrDefault();
            if (bank == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.Banks.Remove(bank);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse(400);
            }
           
            return new APIResponse(200);
        }
    }
}
