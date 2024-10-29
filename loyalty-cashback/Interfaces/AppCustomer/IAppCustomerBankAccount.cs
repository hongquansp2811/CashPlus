﻿using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using System;
using System.Threading.Tasks;

namespace LOYALTY.Interfaces
{
    public interface IAppCustomerBankAccount
    {
        // API lấy danh sách tài khoản ngân hàng
        public APIResponse getListBankAccount(Guid customer_id);
        // API lấy chi tiết tài khoản ngân hàng
        public APIResponse getDetail(Guid id);
        public Task<APIResponse> create(CustomerBankAccountRequest request, string username);
        public APIResponse update(CustomerBankAccountRequest request, string username);
        public APIResponse delete(DeleteGuidRequest request);
        APIResponse getBankDetail(string code = "");
    }
}
