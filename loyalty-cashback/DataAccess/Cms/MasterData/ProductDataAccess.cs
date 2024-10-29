using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using System.Reflection.Emit;
using DocumentFormat.OpenXml.Drawing;

namespace LOYALTY.DataAccess
{
    public class ProductDataAccess : IProduct
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public ProductDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(ProductRequest request)
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
            var lstProduct = (from p in _context.Products
                              join g in _context.ProductGroups on p.product_group_id equals g.id into gs
                              from g in gs.DefaultIfEmpty()
                              join s in _context.Partners on p.partner_id equals s.id
                              orderby p.date_created descending
                              join st in _context.OtherLists on p.status equals st.id into sts
                              from st in sts.DefaultIfEmpty()
                              where p.status_change == true
                              orderby p.number
                              select new
                              {
                                  id = p.id,
                                  code = p.code,
                                  name = p.name,
                                  price = p.price,
                                  product_group_id = p.product_group_id,
                                  product_group_name = g != null ? g.name : "",
                                  partner_id = p.partner_id,
                                  partner_name = s.name,
                                  status = p.status,
                                  status_name = st != null ? st.name : "",
                                  description = p.description,
                                  status_change = p.status_change,
                                  number = p.number
                              });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstProduct = lstProduct.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.description.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            if (request.product_group_id != null)
            {
                lstProduct = lstProduct.Where(x => x.product_group_id == request.product_group_id);
            }

            if (request.status != null)
            {
                lstProduct = lstProduct.Where(x => x.status == request.status);
            }

            if (request.partner_id != null)
            {
                lstProduct = lstProduct.Where(x => x.partner_id == request.partner_id);
            }

            if (request.list_status_not_in != null && request.list_status_not_in.Count > 0)
            {
                lstProduct = lstProduct.Where(x => request.list_status_not_in.Contains((int)x.status) == false);
            }

            // Đếm số lượng
            int countElements = lstProduct.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstProduct.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = (from p in _context.Products
                        join g in _context.ProductGroups on p.product_group_id equals g.id into gs
                        from g in gs.DefaultIfEmpty()
                        join s in _context.Partners on p.partner_id equals s.id
                        orderby p.date_created descending
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            code = p.code,
                            name = p.name,
                            price = p.price,
                            avatar = p.avatar,
                            description = p.description,
                            detail_info = p.detail_info,
                            product_group_id = p.product_group_id,
                            product_group_name = g != null ? g.name : "",
                            partner_id = p.partner_id,
                            partner_name = s.name,
                            partner_created = p.user_created,
                            status = p.status,
                            status_name = st != null ? st.name : "",
                            reason_fail = p.reason_fail,
                            list_images = _context.ProductImages.Where(x => x.product_id == p.id).ToList(),
                            number = p.number,
                            status_change = p.status_change
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(ProductRequest request, string username)
        {
            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.product_group_id == null)
            {
                return new APIResponse("ERROR_PRODUCT_GROUP_ID_MISSING");
            }

            if (request.price == null)
            {
                return new APIResponse("ERROR_PRICE_MISSING");
            }
            if (request.number == null)
            {
                return new APIResponse("ERROR_INDEX_MISSING");
            }
            if (request.status_change == null)
            {
                return new APIResponse("ERROR_STATUS_CHANGE_MISSING");
            }
            var index = _context.Products.Where(x => x.number == request.number && x.partner_id == request.partner_id).FirstOrDefault();
            if (index != null)
            {
                return new APIResponse("Thứ tự hiển thị đã được sử dụng");
            }
            var off_words = _context.offending_Words.ToList();

            string off_word = "";

            foreach (var item in off_words)
            {
                bool off_wordName = request.name.Contains(item.text);
                if (off_wordName)
                {
                    off_word = "Tên sản phẩm có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
                if (request.description != null)
                {
                    bool off_wordDescription = request.description.Contains(item.text);
                    if (off_wordDescription)
                    {
                        off_word = "Tóm tắt sản phẩm có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                        break;
                    }
                }
                if (request.detail_info != null)
                {
                    bool containsDetail_info = request.detail_info.Contains(item.text);
                    if (containsDetail_info)
                    {
                        off_word = "Thông tin chi tiết sản phẩm có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                        break;
                    }
                }
            }

            if (off_word != "")
            {
                return new APIResponse(off_word);
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                var data = new Product();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.avatar = request.avatar;
                data.product_group_id = request.product_group_id;
                data.price = request.price;
                data.description = request.description;
                data.detail_info = request.detail_info;
                data.partner_id = request.partner_id;
                data.status = 3; // Check Trạng thái
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                data.number = request.number;
                data.status_change = request.status_change;
                _context.Products.Add(data);
                _context.SaveChanges();

                for (int i = 0; i < request.list_images.Count; i++)
                {
                    var item = new ProductImage();
                    item.id = Guid.NewGuid();
                    item.product_id = data.id;
                    item.name = request.list_images[i].name;
                    item.links = request.list_images[i].links;
                    _context.ProductImages.Add(item);
                }
                _context.SaveChanges();

                // Tạo thông báo cho admin
                var newNotiAdmin = new Notification();
                newNotiAdmin.id = Guid.NewGuid();
                newNotiAdmin.title = "Thêm mới sản phẩm " + data.code;
                newNotiAdmin.type_id = Guid.Parse("b9168985-8745-4685-84a2-a46f3f7cb2dc");
                newNotiAdmin.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNotiAdmin.date_created = DateTime.Now;
                newNotiAdmin.date_updated = DateTime.Now;
                newNotiAdmin.content = "Tài khoản " + username + " vừa thực hiện thêm mới thông tin sản phẩm: " + request.code + " vào lúc: " + _commonFunction.convertDateToStringFull(DateTime.Now);
                newNotiAdmin.system_type = "CMS_Admin_Noti";
                newNotiAdmin.reference_id = data.id;

                _context.Notifications.Add(newNotiAdmin);
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

        public APIResponse update(ProductRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Products.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.number != data.number)
            {
                var index = _context.Products.Where(x => x.number == request.number && x.partner_id == data.partner_id).FirstOrDefault();
                if (index != null)
                {
                    return new APIResponse("Thứ tự hiển thị đã được sử dụng");
                }
            }
            if (request.number == null)
            {
                return new APIResponse("ERROR_INDEX_MISSING");
            }
            var off_words = _context.offending_Words.ToList();

            string off_word = "";

            foreach (var item in off_words)
            {
                bool off_wordName = request.name.Contains(item.text);
                if (off_wordName)
                {
                    off_word = "Tên sản phẩm có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
                if (request.description != null)
                {
                    bool off_wordDescription = request.description.Contains(item.text);
                    if (off_wordDescription)
                    {
                        off_word = "Tóm tắt sản phẩm có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                        break;
                    }
                }
                if (request.detail_info != null)
                {
                    bool containsDetail_info = request.detail_info.Contains(item.text);
                    if (containsDetail_info)
                    {
                        off_word = "Thông tin chi tiết sản phẩm có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                        break;
                    }
                }
            }

            if (off_word != "")
            {
                return new APIResponse(off_word);
            }

            try
            {
                string oldCode = data.code;

                data.code = request.code;
                data.name = request.name;
                data.product_group_id = request.product_group_id;
                data.price = request.price;
                data.avatar = request.avatar;
                data.description = request.description;
                data.detail_info = request.detail_info;
                data.number = request.number;
                data.status_change = request.status_change;
                data.reason_fail = request.reason_fail;
                data.date_updated = DateTime.Now;
                data.user_updated = username;
                _context.SaveChanges();

                var lstDeletes = _context.ProductImages.Where(x => x.product_id == data.id).ToList();
                _context.ProductImages.RemoveRange(lstDeletes);

                if (data != null && request.list_images != null && request.list_images.Count > 0)
                {
                    for (int i = 0; i < request.list_images.Count; i++)
                    {
                        var item = new ProductImage();
                        item.id = Guid.NewGuid();
                        item.product_id = data.id;
                        item.name = request.list_images[i].name;
                        item.links = request.list_images[i].links;
                        _context.ProductImages.Add(item);
                    }
                }

                // Tạo thông báo cho admin
                var newNotiAdmin = new Notification();
                newNotiAdmin.id = Guid.NewGuid();
                newNotiAdmin.title = "Cập nhập sản phẩm: " + data.code;
                newNotiAdmin.type_id = Guid.Parse("b9168985-8745-4685-84a2-a46f3f7cb2dc");
                newNotiAdmin.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNotiAdmin.date_created = DateTime.Now;
                newNotiAdmin.date_updated = DateTime.Now;
                newNotiAdmin.content = "Tài khoản " + username + " vừa thực hiện thay đổi thông tin sản phẩm: " + oldCode + " vào lúc: " + _commonFunction.convertDateToStringFull(DateTime.Now);
                newNotiAdmin.system_type = "CMS_Admin_Noti";
                newNotiAdmin.reference_id = data.id;

                _context.Notifications.Add(newNotiAdmin);

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
            var data = _context.Products.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.status != 3 && data.status != 6)
            {
                return new APIResponse("ERROR_PRODUCT_STATUS_NOT_DELETE");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                var lstDeletes = _context.ProductImages.Where(x => x.product_id == data.id).ToList();
                _context.ProductImages.RemoveRange(lstDeletes);

                _context.Products.Remove(data);

                // Tạo thông báo cho admin
                var newNotiAdmin = new Notification();
                newNotiAdmin.id = Guid.NewGuid();
                newNotiAdmin.title = "Xóa sản phẩm: " + data.code;
                newNotiAdmin.type_id = Guid.Parse("b9168985-8745-4685-84a2-a46f3f7cb2dc");
                newNotiAdmin.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNotiAdmin.date_created = DateTime.Now;
                newNotiAdmin.date_updated = DateTime.Now;
                newNotiAdmin.content = "Tài khoản " + username + " vừa thực hiện xóa sản phẩm với mã sản phẩm là: " + data.code + " vào lúc : " + _commonFunction.convertDateToStringFull(DateTime.Now);
                newNotiAdmin.system_type = "CMS_Admin_Noti";
                newNotiAdmin.reference_id = data.id;

                _context.Notifications.Add(newNotiAdmin);

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

        public APIResponse sendApprove(DeleteGuidRequest req, string username)
        {
            var data = _context.Products.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.status != 3 && data.status != 6)
            {
                return new APIResponse("ERROR_PRODUCT_STATUS_NOT_CHANGE");
            }
            if (data.status_change == false || data.status_change == null) return new APIResponse("Vui lòng cập nhật sản phẩm sang trạng thái hiển thị để gửi duyệt");

            try
            {
                data.status = 4;

                // Tạo thông báo cho admin
                var newNotiAdmin = new Notification();
                newNotiAdmin.id = Guid.NewGuid();
                newNotiAdmin.title = "Yêu cầu duyệt sản phẩm: " + data.code;
                newNotiAdmin.type_id = Guid.Parse("b9168985-8745-4685-84a2-a46f3f7cb2dc");
                newNotiAdmin.user_id = Guid.Parse(Consts.USER_ADMIN_ID);
                newNotiAdmin.date_created = DateTime.Now;
                newNotiAdmin.date_updated = DateTime.Now;
                newNotiAdmin.content = "Tài khoản " + username + " vừa thực hiện gửi duyệt thông tin sản phẩm với mã sản phẩm là : " + data.code + " vào lúc : " + _commonFunction.convertDateToStringFull(DateTime.Now);
                newNotiAdmin.system_type = "CMS_Admin_Noti";
                newNotiAdmin.reference_id = data.id;

                _context.Notifications.Add(newNotiAdmin);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public APIResponse approve(DeleteGuidRequest req)
        {
            var data = _context.Products.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.status != 4)
            {
                return new APIResponse("ERROR_PRODUCT_STATUS_NOT_CHANGE");
            }

            try
            {
                data.status = 5;

                // Tạo thông báo cho admin
                var newNotiAdmin = new Notification();
                newNotiAdmin.id = Guid.NewGuid();
                newNotiAdmin.title = "Phê duyệt sản phẩm: " + data.code;
                newNotiAdmin.type_id = Guid.Parse("16fe077c-d9fd-45a3-be22-ffe0f7df6361");
                newNotiAdmin.user_id = data.partner_id;
                newNotiAdmin.date_created = DateTime.Now;
                newNotiAdmin.date_updated = DateTime.Now;
                var noti = _context.NotiConfigs.Select(p => p.Product_Acp).FirstOrDefault();
                //newNotiAdmin.content = "Sản phẩm: " + data.name + " đã được phê duyệt vào lúc: " + _commonFunction.convertDateToStringFull(DateTime.Now);
                newNotiAdmin.content = noti.Replace("{TenSanPham}", data.name).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now));

                newNotiAdmin.system_type = "CMS_Partner_Noti";
                newNotiAdmin.reference_id = data.id;

                _context.Notifications.Add(newNotiAdmin);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public APIResponse denied(DeleteGuidRequest req)
        {
            var data = _context.Products.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.status != 4)
            {
                return new APIResponse("ERROR_PRODUCT_STATUS_NOT_CHANGE");
            }

            try
            {
                data.status = 6;
                data.reason_fail = req.reason_fail;

                // Tạo thông báo cho admin
                var newNotiAdmin = new Notification();
                newNotiAdmin.id = Guid.NewGuid();
                newNotiAdmin.title = "Từ chối sản phẩm: " + data.code;
                newNotiAdmin.type_id = Guid.Parse("16fe077c-d9fd-45a3-be22-ffe0f7df6361");
                newNotiAdmin.user_id = data.partner_id;
                newNotiAdmin.date_created = DateTime.Now;
                newNotiAdmin.date_updated = DateTime.Now;
                var noti = _context.NotiConfigs.Select(p => p.Product_De).FirstOrDefault();
                //newNotiAdmin.content = "Sản phẩm: " + data.name + " đã bị từ chối vào lúc: " + _commonFunction.convertDateToStringFull(DateTime.Now) + " bởi lý do:" + req.reason_fail;
                newNotiAdmin.content = noti.Replace("{TenSanPham}", data.name).Replace("{Time}", _commonFunction.convertDateToStringFull(DateTime.Now)).Replace("{LyDoTuChoi}", req.reason_fail);
                newNotiAdmin.system_type = "CMS_Partner_Noti";
                newNotiAdmin.reference_id = data.id;

                _context.Notifications.Add(newNotiAdmin);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }

        public APIResponse getListWeb(ProductRequest request)
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
            var lstProduct = (from p in _context.Products
                              join g in _context.ProductGroups on p.product_group_id equals g.id into gs
                              from g in gs.DefaultIfEmpty()
                              join s in _context.Partners on p.partner_id equals s.id
                              join st in _context.OtherLists on p.status equals st.id into sts
                              from st in sts.DefaultIfEmpty()
                              orderby p.date_created descending
                              select new
                              {
                                  id = p.id,
                                  code = p.code,
                                  name = p.name,
                                  price = p.price,
                                  product_group_id = p.product_group_id,
                                  product_group_name = g != null ? g.name : "",
                                  partner_id = p.partner_id,
                                  partner_name = s.name,
                                  status = p.status,
                                  status_name = st != null ? st.name : "",
                                  description = p.description,
                                  status_change = p.status_change,
                                  number = p.number
                              });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstProduct = lstProduct.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.description.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            if (request.product_group_id != null)
            {
                lstProduct = lstProduct.Where(x => x.product_group_id == request.product_group_id);
            }

            if (request.status != null)
            {
                lstProduct = lstProduct.Where(x => x.status == request.status);
            }

            if (request.partner_id != null)
            {
                lstProduct = lstProduct.Where(x => x.partner_id == request.partner_id);
            }

            if (request.list_status_not_in != null && request.list_status_not_in.Count > 0)
            {
                lstProduct = lstProduct.Where(x => request.list_status_not_in.Contains((int)x.status) == false);
            }

            // Đếm số lượng
            int countElements = lstProduct.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstProduct.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList.OrderBy(p => p.number)};
            return new APIResponse(dataResult);
        }
    }
}
