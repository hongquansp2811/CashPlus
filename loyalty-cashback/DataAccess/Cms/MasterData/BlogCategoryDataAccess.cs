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
    public class BlogCategoryDataAccess : IBlogCategory
    {
        private readonly LOYALTYContext _context;
        public BlogCategoryDataAccess(LOYALTYContext context) 
        {
            this._context = context;
        }

        public APIResponse getList(BlogCategoryRequest request)
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
            var lstBlogCategory = (from p in _context.BlogCategorys
                                   orderby p.date_created descending
                                   select new
                                   {
                                       id = p.id,
                                       code = p.code,
                                       name = p.name,
                                       description = p.description
                                   });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstBlogCategory = lstBlogCategory.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            // Đếm số lượng
            int countElements = lstBlogCategory.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstBlogCategory.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = _context.BlogCategorys.Where(x => x.id == id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(BlogCategoryRequest request, string username)
        {
            if (request.code == null)
            {
                return new APIResponse("ERROR_CODE_MISSING");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }


            var dataCode = _context.BlogCategorys.Where(x => x.code == request.code).FirstOrDefault();

            if (dataCode != null)
            {
                return new APIResponse("ERROR_CODE_EXISTS");
            }

            try
            {
                var data = new BlogCategory();
                data.id = Guid.NewGuid();
                data.code = request.code;
                data.name = request.name;
                data.title_url = Strings.RemoveDiacriticUrls(request.name).ToLower();
                data.description = request.description;
                data.avatar = request.avatar;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.BlogCategorys.Add(data);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }
         
            return new APIResponse(200);
        }

        public APIResponse update(BlogCategoryRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.BlogCategorys.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.name != null && request.name.Length > 0)
            {
                data.name = request.name;
            }

            try
            {
                data.name = request.name;
                data.title_url = Strings.RemoveDiacriticUrls(request.name).ToLower();
                data.description = request.description;
                data.avatar = request.avatar;
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req)
        {
            var data = _context.BlogCategorys.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.BlogCategorys.Remove(data);
                _context.SaveChanges();
            } catch(Exception ex)
            {
                return new APIResponse(400);
            }
           
            return new APIResponse(200);
        }
    }
}
