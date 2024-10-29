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
    public class BlogDataAccess : IBlog
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public BlogDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(BlogRequest request)
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
            var lstBlog = (from p in _context.Blogs
                           join c in _context.BlogCategorys on p.blog_category_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               title = p.title,
                               blog_category_id = p.blog_category_id,
                               blog_category_name = c != null ? c.name : "",
                               date_blog = _commonFunction.convertDateToStringSort(p.date_blog),
                               date_blog_origin = p.date_blog,
                               description = p.description
                           });

            // Nếu tồn tại Where theo tên
            if (request.title != null && request.title.Length > 0)
            {
                lstBlog = lstBlog.Where(x => x.title.Trim().ToLower().Contains(request.title.Trim().ToLower()) || x.description.Trim().ToLower().Contains(request.title.Trim().ToLower()));
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstBlog = lstBlog.Where(x => x.date_blog_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstBlog = lstBlog.Where(x => x.date_blog_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            if (request.blog_category_id != null)
            {
                lstBlog = lstBlog.Where(x => x.blog_category_id == request.blog_category_id);
            }

            // Đếm số lượng
            int countElements = lstBlog.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstBlog.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(Guid id)
        {
            var data = (from p in _context.Blogs
                        join c in _context.BlogCategorys on p.blog_category_id equals c.id into cs
                        from c in cs.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            title = p.title,
                            blog_category_id = p.blog_category_id,
                            blog_category_name = c != null ? c.name : "",
                            date_blog = _commonFunction.convertDateToStringSort(p.date_blog),
                            date_blog_origin = p.date_blog,
                            description = p.description,
                            content = p.content,
                            files = p.files,
                            avatar = p.avatar
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(BlogRequest request, string username)
        {
            if (request.title == null)
            {
                return new APIResponse("ERROR_TITLE_MISSING");
            }

            if (request.blog_category_id == null)
            {
                return new APIResponse("ERROR_BLOG_CATEGORY_ID_MISSING");
            }

            if (request.description == null)
            {
                return new APIResponse("ERROR_DESCRIPTION_MISSING");
            }

            if (request.content == null)
            {
                return new APIResponse("ERROR_CONTENT_MISSING");
            }

            var off_words = _context.offending_Words.ToList();

            string off_word = "";

            foreach (var item in off_words)
            {
                bool off_wordTitle = request.title.Contains(item.text);
                if (off_wordTitle)
                {
                    off_word = "Tiêu đề bài viết có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
                bool off_wordDescription = request.description.Contains(item.text);
                if (off_wordDescription)
                {
                    off_word = "Mô tả bài viết có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
                bool off_wordContent = request.content.Contains(item.text);
                if (off_wordContent)
                {
                    off_word = "Nội dung bài viết có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
            }

            if (off_word != "")
            {
                return new APIResponse(off_word);
            }
            try
            {
                var data = new Blog();
                data.id = Guid.NewGuid();
                data.title = request.title;
                data.title_url = Strings.RemoveDiacriticUrls(request.title).ToLower();
                data.blog_category_id = request.blog_category_id;
                data.files = request.files;
                data.description = request.description;
                data.content = request.content;
                data.avatar = request.avatar;

                if (request.date_blog != null)
                {
                    data.date_blog = _commonFunction.convertStringSortToDate(request.date_blog);
                }

                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.Blogs.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(BlogRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Blogs.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            var off_words = _context.offending_Words.ToList();

            string off_word = "";

            foreach (var item in off_words)
            {
                bool off_wordTitle = request.title.Contains(item.text);
                if (off_wordTitle)
                {
                    off_word = "Tiêu đề bài viết có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
                bool off_wordDescription = request.description.Contains(item.text);
                if (off_wordDescription)
                {
                    off_word = "Mô tả bài viết có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
                bool off_wordContent = request.content.Contains(item.text);
                if (off_wordContent)
                {
                    off_word = "Nội dung bài viết có chưa từ ngữ vi phạm: " + item.text + ".Vui lòng cập nhập nội dung khác";
                    break;
                }
            }

            if (off_word != "")
            {
                return new APIResponse(off_word);
            }

            try
            {
                data.title = request.title;
                data.title_url = Strings.RemoveDiacriticUrls(request.title).ToLower();
                data.blog_category_id = request.blog_category_id;
                data.files = request.files;
                data.description = request.description;
                data.content = request.content;
                data.avatar = request.avatar;

                if (request.date_blog != null)
                {
                    data.date_blog = _commonFunction.convertStringSortToDate(request.date_blog);
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req)
        {
            var data = _context.Blogs.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.Blogs.Remove(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }
    }
}
