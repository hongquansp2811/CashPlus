﻿using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using LOYALTY.PaymentGate;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataAccess
{
    public class AppPartnerBankAccountDataAccess : IPartnerBankAccount
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly BKTransaction _bkTransaction;

        public AppPartnerBankAccountDataAccess(LOYALTYContext context, ICommonFunction commonFunction, BKTransaction bkTransaction)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _bkTransaction = bkTransaction;
        }

        public APIResponse getListBankAccount(Guid partner_id)
        {
            var lstData = (from p in _context.CustomerBankAccounts
                           join b in _context.Banks on p.bank_id equals b.id
                           where p.user_id == partner_id
                           select new
                           {
                               id = p.id,
                               bank_no = p.bank_no,
                               bank_id = p.bank_id,
                               bank_name = b.name,
                               bank_owner = p.bank_owner,
                               bank_avatar = b.avatar,
                               bank_background = b.background,
                               bank_branch = p.bank_branch
                           }).ToList();
            return new APIResponse(lstData);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = (from p in _context.CustomerBankAccounts
                          join b in _context.Banks on p.bank_id equals b.id
                          where p.id == id
                          select new
                          {
                              id = p.id,
                              bank_no = p.bank_no,
                              bank_id = p.bank_id,
                              bank_name = b.name,
                              bank_owner = p.bank_owner,
                              bank_branch = p.bank_branch,
                              is_default = p.is_default
                          }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public async Task<APIResponse> create(CustomerBankAccountRequest request, string username)
        {
            if (request.bank_id == null)
            {
                return new APIResponse("ERROR_BANK_ID_MISSING");
            }

            if (request.bank_no == null)
            {
                return new APIResponse("ERROR_BANK_NO_MISSING");
            }

            if (request.user_id == null)
            {
                return new APIResponse("ERROR_USER_ID_MISSING");
            }

            var dataCode = _context.CustomerBankAccounts.Where(x => x.user_id == request.user_id && x.bank_no == request.bank_no && x.bank_id == request.bank_id).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            if (request.bank_owner == null)
            {
                return new APIResponse("ERROR_BANK_OWNER_MISSING");
            }

            if (request.secret_key == null)
            {
                return new APIResponse("ERROR_SECRET_KEY_MISSING");
            }

            var userObj = _context.Users.Where(x => x.partner_id == request.user_id && x.username == username).FirstOrDefault();

            if (userObj == null)
            {
                return new APIResponse("ERROR_USER_NOT_EXISTS");
            }

            //if (userObj.secret_key == null)
            //{
            //    return new APIResponse("ERROR_USER_NOT_HAVE_SECRET_KEY");
            //}

            if (userObj.secret_key != null)
            {
                if (userObj.secret_key != _commonFunction.ComputeSha256Hash(request.secret_key))
                {
                    return new APIResponse("ERROR_SECRET_KEY_INCORRECT");
                }
            }

            var bank_code = _context.Banks.FirstOrDefault(x => x.id == request.bank_id)?.bank_code;
            var checkExist = await _bkTransaction.checkAccountExist(bank_code, request.bank_no, request.bank_owner);

            if (!string.IsNullOrEmpty(checkExist))
            {
                return new APIResponse(checkExist);
            }

            try
            {
                var data = new CustomerBankAccount();
                data.id = Guid.NewGuid();
                data.user_id = request.user_id;
                data.bank_id = request.bank_id;
                data.bank_no = request.bank_no;
                data.bank_owner = request.bank_owner.ToUpper();
                data.bank_branch = request.bank_branch;
                data.is_default = request.is_default != null ? request.is_default : false;
                data.type_id = 1;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.CustomerBankAccounts.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(CustomerBankAccountRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }

            var data = _context.CustomerBankAccounts.Where(x => x.id == request.id).FirstOrDefault();
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

            var dataCode = _context.CustomerBankAccounts.Where(x => x.user_id == request.user_id && x.bank_no == request.bank_no && x.bank_id == request.bank_id && x.id != request.id).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            if (request.bank_owner == null)
            {
                return new APIResponse("ERROR_BANK_OWNER_MISSING");
            }

            if (request.secret_key == null)
            {
                return new APIResponse("ERROR_SECRET_KEY_MISSING");
            }

            var userObj = _context.Users.Where(x => x.partner_id == request.user_id && x.username == username).FirstOrDefault();

            if (userObj == null)
            {
                return new APIResponse("ERROR_USER_NOT_EXISTS");
            }

            if (userObj.secret_key == null)
            {
                return new APIResponse("ERROR_USER_NOT_HAVE_SECRET_KEY");
            }

            if (userObj.secret_key != _commonFunction.ComputeSha256Hash(request.secret_key))
            {
                return new APIResponse("ERROR_SECRET_KEY_INCORRECT");
            }

            var bank_code = _context.Banks.FirstOrDefault(x => x.id == request.bank_id)?.bank_code;
            var checkExist = _bkTransaction.checkAccountExist(bank_code, request.bank_no, request.bank_owner).Result;

            if (!string.IsNullOrEmpty(checkExist))
            {
                return new APIResponse(checkExist);
            }

            try
            {
                data.bank_id = request.bank_id;
                data.bank_no = request.bank_no;
                data.bank_owner = request.bank_owner.ToUpper();
                data.bank_branch = request.bank_branch;
                data.is_default = request.is_default != null ? request.is_default : false;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req, string username)
        {
            var data = _context.CustomerBankAccounts.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (req.secret_key == null)
            {
                return new APIResponse("ERROR_SECRET_KEY_MISSING");
            }

            var userObj = _context.Users.Where(x => x.partner_id == data.user_id && x.username == username).FirstOrDefault();

            if (userObj == null)
            {
                return new APIResponse("ERROR_USER_NOT_EXISTS");
            }

            if (userObj.secret_key == null)
            {
                return new APIResponse("ERROR_USER_NOT_HAVE_SECRET_KEY");
            }

            if (userObj.secret_key != _commonFunction.ComputeSha256Hash(req.secret_key))
            {
                return new APIResponse("ERROR_SECRET_KEY_INCORRECT");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.CustomerBankAccounts.Remove(data);
                _context.SaveChanges();

                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }
        public APIResponse getBankDetail(string code = "")
        {
            var bank = _context.Banks.Where(x => code.Equals(x.bank_code)).FirstOrDefault();
            if (bank == null)
            {
                return new APIResponse("ERROR_BANK_NOT_EXIST");
            }
            return new APIResponse(bank);
        }
    }
}