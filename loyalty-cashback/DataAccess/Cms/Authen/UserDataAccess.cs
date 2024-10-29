using System;
using System.Linq;
using System.Collections.Generic;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using System.Text.RegularExpressions;

namespace LOYALTY.DataAccess
{
    public class UserDataAccess : IUser
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public UserDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(UserRequest request)
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

            var lstUser = (from p in _context.Users
                           join f in _context.UserGroups on p.user_group_id equals f.id into fs
                           from f in fs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.is_admin == true && p.is_delete != true
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               username = p.username,
                               code = p.code,
                               full_name = p.full_name,
                               phone = p.phone,
                               email = p.email,
                               user_group_id = p.user_group_id,
                               user_group_name = f != null ? f.name : "",
                               status = p.status,
                               status_name = st != null ? st.name : "",
                               date_updated = _commonFunction.convertDateToStringSort(p.date_updated)
                           });
            // Nếu tồn tại Where theo tên
            if (request.full_name != null && request.full_name.Length > 0)
            {
                lstUser = lstUser.Where(x => x.username.Trim().ToLower().Contains(request.full_name.Trim().ToLower()) || x.full_name.Trim().ToLower().Contains(request.full_name.Trim().ToLower())
                || x.email.Trim().ToLower().Contains(request.full_name.Trim().ToLower()) || x.code.Trim().ToLower().Contains(request.full_name.Trim().ToLower()));
            }

            if (request.user_group_id != null)
            {
                lstUser = lstUser.Where(x => x.user_group_id == request.user_group_id);
            }

            if (request.status != null)
            {
                lstUser = lstUser.Where(x => x.status == request.status);
            }

            // Đếm số lượng
            int countElements = lstUser.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstUser.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            List<object> userPermissions = new List<object>();
            var action = (from p in _context.Users
                          where p.id == id && p.is_admin == true
                          select new
                          {
                              id = p.id,
                              username = p.username,
                              code = p.code,
                              full_name = p.full_name,
                              avatar = p.avatar,
                              user_group_id = p.user_group_id,
                              email = p.email,
                              phone = p.phone,
                              status = p.status,
                              userPermissions = (from i in _context.UserPermissions
                                                 join f in _context.Actions on i.action_id equals f.id into fs
                                                 from f in fs.DefaultIfEmpty()
                                                 join g in _context.Functions on f.function_id equals g.id into gs
                                                 from g in gs.DefaultIfEmpty()
                                                 where i.user_id == p.id
                                                 select new
                                                 {
                                                     action_id = i.action_id,
                                                     action_name = f == null ? "" : f.name,
                                                     function_id = g == null ? null : g.id,
                                                     function_name = g == null ? null : g.name
                                                 }).ToList()
                          }).FirstOrDefault();

            if (action == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            return new APIResponse(action);
        }

        public APIResponse create(UserRequest request, string username)
        {

            if (request.username == null)
            {
                return new APIResponse("ERROR_USERNAME_MISSING");
            }

            var dataSame = _context.Users.Where(x => x.username == request.username && x.is_admin == true).FirstOrDefault();

            if (dataSame != null)
            {
                return new APIResponse("ERROR_USERNAME_EXIST");
            }

            if (request.full_name == null)
            {
                return new APIResponse("ERROR_FULLNAME_MISSING");
            }

            if (request.email == null)
            {
                return new APIResponse("ERROR_EMAIL_MISSING");
            }

            if (request.phone == null)
            {
                return new APIResponse("ERROR_PHONE_MISSING");
            }

            if (request.password == null)
            {
                return new APIResponse("ERROR_PASSWORD_MISSING");
            }

            if (request.user_group_id == null)
            {
                return new APIResponse("ERROR_USERGROUP_ID_MISSING");
            }

            if (request.userPermissions == null || request.userPermissions.Count == 0)
            {
                return new APIResponse("ERROR_PERMISSION_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            if (!_commonFunction.ValidatePassword(request.password))
            {
                return new APIResponse("ERROR_PASSWORD_INVALID");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var data = new User();
                data.id = Guid.NewGuid();
                var maxCodeObject = _context.Users.Where(x => x.is_admin == true && x.code != null && x.code.Contains("NV")).OrderByDescending(x => x.code).FirstOrDefault();
                string codeEmployee = "";
                if (maxCodeObject == null)
                {
                    codeEmployee = "NV00000001";
                }
                else
                {
                    string maxCode = maxCodeObject.code;
                    maxCode = maxCode.Substring(2);
                    int orders = int.Parse(maxCode);
                    orders = orders + 1;
                    string orderString = orders.ToString();
                    char pad = '0';
                    int number = 8;
                    codeEmployee = "NV" + orderString.PadLeft(number, pad);
                }
                data.code = codeEmployee;
                data.full_name = request.full_name;
                data.avatar = request.avatar;
                data.user_group_id = request.user_group_id;
                data.email = request.email;
                data.phone = request.phone;
                data.username = request.username;
                data.password = _commonFunction.ComputeSha256Hash(request.password);
                data.status = 1;
                data.is_sysadmin = false;
                data.is_admin = true;
                data.is_customer = false;
                data.is_partner_admin = false;
                data.is_partner = false;
                data.is_delete = false;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.Users.Add(data);
                // Save Changes
                _context.SaveChanges();

                for (int i = 0; i < request.userPermissions.Count; i++)
                {
                    var item = new UserPermission();
                    item.id = Guid.NewGuid();
                    item.user_id = data.id;
                    item.action_id = request.userPermissions[i].action_id;
                    _context.UserPermissions.Add(item);
                }

                _context.SaveChanges();

                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(UserRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }

            if (request.full_name == null)
            {
                return new APIResponse("ERROR_FULLNAME_MISSING");
            }

            if (request.email == null)
            {
                return new APIResponse("ERROR_EMAIL_MISSING");
            }

            if (request.phone == null)
            {
                return new APIResponse("ERROR_PHONE_MISSING");
            }

            if (request.user_group_id == null)
            {
                return new APIResponse("ERROR_USERGROUP_ID_MISSING");
            }

            if (request.userPermissions == null || request.userPermissions.Count == 0)
            {
                return new APIResponse("ERROR_PERMISSION_MISSING");
            }
            var data = _context.Users.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            if (request.password != null && request.password.Length > 0)
            {
                if (!_commonFunction.ValidatePassword(request.password))
                {
                    return new APIResponse("ERROR_PASSWORD_INVALID");
                }
            }
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                data.full_name = request.full_name;
                data.avatar = request.avatar;
                data.user_group_id = request.user_group_id;
                data.email = request.email;
                data.phone = request.phone;
                if (request.password != null && request.password.Length > 0)
                {
                    data.password = _commonFunction.ComputeSha256Hash(request.password);
                }

                if (request.status != null)
                {
                    data.status = request.status;
                }

                var lstPermissionDeletes = _context.UserPermissions.Where(x => x.user_id == data.id).ToList();
                _context.UserPermissions.RemoveRange(lstPermissionDeletes);

                if (data != null && request.userPermissions != null && request.userPermissions.Count > 0)
                {
                    for (int i = 0; i < request.userPermissions.Count; i++)
                    {
                        var item = new UserPermission();
                        item.id = Guid.NewGuid();
                        item.user_id = data.id;
                        item.action_id = request.userPermissions[i].action_id;
                        _context.UserPermissions.Add(item);
                    }
                }

                var newNoti1 = new Notification();
                newNoti1.id = Guid.NewGuid();
                newNoti1.title = "Thay đổi khóa";
                newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                newNoti1.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNoti1.date_created = DateTime.Now;
                newNoti1.date_updated = DateTime.Now;
                newNoti1.content = "Tài khoản " + data.full_name + " đã bị thay đổi thông tin, vui lòng đăng nhập lại!";
                newNoti1.system_type = "Change_Permissions";
                newNoti1.reference_id = data.id;
                _context.Notifications.Add(newNoti1);

                _context.SaveChanges();

                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();

                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req)
        {
            var data = _context.Users.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.username == "administrator")
            {
                return new APIResponse("ERROR_DELETE_ADMIN");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                data.is_delete = true;

                var newNoti1 = new Notification();
                newNoti1.id = Guid.NewGuid();
                newNoti1.title = "Thay đổi xóa";
                newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                newNoti1.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNoti1.date_created = DateTime.Now;
                newNoti1.date_updated = DateTime.Now;
                newNoti1.content = "Tài khoản " + data.full_name + " đã bị xóa, vui lòng đăng nhập lại!";
                newNoti1.system_type = "Lock_Account";
                newNoti1.reference_id = data.id;
                _context.Notifications.Add(newNoti1);

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

        public APIResponse lockAccount(DeleteGuidRequest req)
        {
            var data = _context.Users.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.username == "administrator")
            {
                return new APIResponse("ERROR_LOCK_ADMIN");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                data.status = 2;

                var newNoti1 = new Notification();
                newNoti1.id = Guid.NewGuid();
                newNoti1.title = "Thay đổi khóa";
                newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                newNoti1.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNoti1.date_created = DateTime.Now;
                newNoti1.date_updated = DateTime.Now;
                newNoti1.content = "Tài khoản " + data.full_name + " đã bị khóa, vui lòng đăng nhập lại!";
                newNoti1.system_type = "Lock_Account";
                newNoti1.reference_id = data.id;
                _context.Notifications.Add(newNoti1);

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

        public APIResponse unlockAccount(DeleteGuidRequest req)
        {
            var data = _context.Users.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.username == "administrator")
            {
                return new APIResponse("ERROR_LOCK_ADMIN");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                data.status = 1;

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

        public APIResponse changePass(DeleteGuidRequest req)
        {
            var data = _context.Users.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.username == "administrator")
            {
                return new APIResponse("ERROR_CHANGE_PASS_ADMIN");
            }

            if (req.new_password == null || req.new_password.Length == 0)
            {
                return new APIResponse("ERROR_NEW_PASSWORD_MISSING");
            }

            if (!_commonFunction.ValidatePassword(req.new_password))
            {
                return new APIResponse("ERROR_PASSWORD_INVALID");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                data.password = _commonFunction.ComputeSha256Hash(req.new_password);

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
    }
}
