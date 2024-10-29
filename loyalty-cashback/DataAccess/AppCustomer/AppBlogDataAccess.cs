using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class AppBlogDataAccess : IAppBlog
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppBlogDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getListCategory()
        {
            var lstData = (from p in _context.BlogCategorys
                           select new
                           {
                               id = p.id,
                               name = p.name,
                               avatar = p.avatar
                           }).ToList();

            return new APIResponse(lstData);
        }

        public APIResponse getDetailBlog(Guid id)
        {
            var data = (from p in _context.Blogs
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            tit = p.title,
                            avatar = p.avatar,
                            content = p.content,
                            blog_category_id = p.blog_category_id,
                            other_blogs = (from oth in _context.Blogs
                                           where oth.id != p.id && oth.blog_category_id == p.blog_category_id
                                           orderby oth.date_blog descending
                                           select new
                                           {
                                               id = oth.id,
                                               tit = oth.title,
                                               avatar = oth.avatar,
                                               date_blog = oth.date_blog != null ? _commonFunction.convertDateToStringSort(oth.date_blog) : "",
                                               content = oth.content
                                           }).Take(5).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                var dataBlog = _context.Blogs.Where(x => x.id == id).FirstOrDefault();

                if (dataBlog.views == null)
                {
                    dataBlog.views = 1;
                }
                else
                {
                    dataBlog.views += 1;
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {

            }

            return new APIResponse(data);
        }

        public APIResponse getListBlog(BlogRequest request)
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
            var lstData = (from p in _context.Blogs
                           orderby p.date_blog descending
                           select new
                           {
                               id = p.id,
                               blog_category_id = p.blog_category_id,
                               tit = p.title,
                               avatar = p.avatar,
                               description = p.description,
                               date_blog = p.date_blog != null ? _commonFunction.convertDateToStringSort(p.date_blog) : ""
                           });

            if (request.blog_category_id != null)
            {
                lstData = lstData.Where(x => x.blog_category_id == request.blog_category_id);
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
    }
}
