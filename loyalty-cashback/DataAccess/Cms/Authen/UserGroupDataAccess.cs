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
using DocumentFormat.OpenXml.Spreadsheet;

namespace LOYALTY.DataAccess
{
    public class UserGroupDataAccess : IUserGroup
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public UserGroupDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(UserGroupRequest request)
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

            var lstUserGroup = (from p in _context.UserGroups
                                orderby p.date_created descending
                                select new
                                {
                                    id = p.id,
                                    code = p.code,
                                    name = p.name,
                                    description = p.description,
                                    status = p.status,
                                    date_created = _commonFunction.convertDateToStringSort(p.date_created)
                                });
            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstUserGroup = lstUserGroup.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            // Đếm số lượng
            int countElements = lstUserGroup.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstUserGroup.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            List<object> userGroupPermissions = new List<object>();
            var action = (from p in _context.UserGroups
                          where p.id == id
                          select new
                          {
                              id = p.id,
                              code = p.code,
                              name = p.name,
                              description = p.description,
                              status = p.status,
                              userGroupPermissions = (from i in _context.UserGroupPermissions
                                                      join f in _context.Actions on i.action_id equals f.id into fs
                                                      from f in fs.DefaultIfEmpty()
                                                      join g in _context.Functions on f.function_id equals g.id into gs
                                                      from g in gs.DefaultIfEmpty()
                                                      where i.user_group_id == p.id
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

        public APIResponse create(UserGroupRequest request, string username)
        {

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            var dataSame = _context.UserGroups.Where(x => x.name == request.name).FirstOrDefault();

            if (dataSame != null)
            {
                return new APIResponse("ERROR_NAME_EXIST");

            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var data = new UserGroup();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.description = request.description;
                data.status = 1;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.UserGroups.Add(data);
                // Save Changes
                _context.SaveChanges();

                for (int i = 0; i < request.userGroupPermissions.Count; i++)
                {
                    var item = new UserGroupPermission();
                    item.id = Guid.NewGuid();
                    item.user_group_id = data.id;
                    item.action_id = request.userGroupPermissions[i].action_id;
                    _context.UserGroupPermissions.Add(item);
                }

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

        public APIResponse update(UserGroupRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }

            var data = _context.UserGroups.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {

                data.name = request.name;
                data.description = request.description;
                data.status = request.status;
                data.code = request.code;
                var lstPermissionDeletes = _context.UserGroupPermissions.Where(x => x.user_group_id == data.id).ToList();
                _context.UserGroupPermissions.RemoveRange(lstPermissionDeletes);

                if (data != null && request.userGroupPermissions != null && request.userGroupPermissions.Count > 0)
                {
                    for (int i = 0; i < request.userGroupPermissions.Count; i++)
                    {
                        var item = new UserGroupPermission();
                        item.id = Guid.NewGuid();
                        item.user_group_id = data.id;
                        item.action_id = request.userGroupPermissions[i].action_id;
                        _context.UserGroupPermissions.Add(item);
                    }
                }

                var dataUser = _context.Users.Where(l => l.user_group_id == request.id).ToList();

                List<Notification> lst = new List<Notification>();

                dataUser.ForEach(item =>
                {
                    var newNoti1 = new Notification();
                    newNoti1.id = Guid.NewGuid();
                    newNoti1.title = "Thay đổi phân quyền";
                    newNoti1.type_id = Guid.Parse("16FE077C-D9FD-45A3-BE22-FFE0F7DF6361");
                    newNoti1.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                    newNoti1.date_created = DateTime.Now;
                    newNoti1.date_updated = DateTime.Now;
                    newNoti1.content = "Tài khoản " + item.full_name + " đã bị thay đổi phân quyền, vui lòng đăng nhập lại!";
                    newNoti1.system_type = "Change_Permissions";
                    newNoti1.reference_id = item.id;

                    lst.Add(newNoti1);
                });



                _context.Notifications.AddRange(lst);
                _context.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req)
        {
            var data = _context.UserGroups.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var lstPermissions = _context.UserGroupPermissions.Where(x => x.user_group_id == req.id).ToList();
                _context.UserGroupPermissions.RemoveRange(lstPermissions);

                _context.SaveChanges();

                _context.UserGroups.Remove(data);

                _context.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {

                transaction.Rollback();

                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public APIResponse getPermission(Guid id)
        {
            List<object> userGroupPermissions = new List<object>();
            var action = (from p in _context.UserGroups
                          where p.id == id
                          select new
                          {
                              id = p.id,
                              name = p.name,
                              userGroupPermissions = (from i in _context.UserGroupPermissions
                                                      join f in _context.Actions on i.action_id equals f.id into fs
                                                      from f in fs.DefaultIfEmpty()
                                                      join g in _context.Functions on f.function_id equals g.id into gs
                                                      from g in gs.DefaultIfEmpty()
                                                      where i.user_group_id == p.id
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
    }
}
