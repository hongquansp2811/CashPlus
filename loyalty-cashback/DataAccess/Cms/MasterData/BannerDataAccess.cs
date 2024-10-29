using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.Helpers;

namespace LOYALTY.DataAccess
{
    public class BannerDataAccess : IBanner
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public BannerDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            this._commonFunction = commonFunction;
        }

        public APIResponse getList(BannerRequest request)
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
            // Khai báo mảng ban đầu
            var lstBanner = (from p in _context.Banners
                                   orderby p.date_created descending
                                   select new
                                   {
                                       id = p.id,
                                       title = p.title,
                                       title_url = p.title_url,
                                       image_link = p.image_link,
                                       orders = p.orders,
                                       per_click = p.per_click,
                                       start_date = _commonFunction.convertDateToStringSort(p.start_date),
                                       start_date_origin = p.start_date,
                                       end_date = _commonFunction.convertDateToStringSort(p.end_date),
                                       end_date_origin = p.end_date,
                                       user_created = p.user_created,
                                       status = p.status
                                   });

            // Nếu tồn tại Where theo tên
            if (request.title != null && request.title.Length > 0)
            {
                lstBanner = lstBanner.Where(x => x.title.Trim().ToLower().Contains(request.title.Trim().ToLower()));
            }

            // Nếu tồn tại Where theo tên
            if (request.start_date != null)
            {
                lstBanner = lstBanner.Where(x => x.start_date_origin >= _commonFunction.convertStringSortToDate(request.start_date));
            }

            if (request.end_date != null)
            {
                lstBanner = lstBanner.Where(x => x.end_date_origin <= _commonFunction.convertStringSortToDate(request.end_date));
            }

            if (request.status != null)
            {
                lstBanner = lstBanner.Where(x => x.status == request.status);
            }

            // Đếm số lượng
            int countElements = lstBanner.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstBanner.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(int id)
        {
            var data = (from p in _context.Banners
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            title = p.title,
                            title_url = p.title_url,
                            image_link = p.image_link,
                            content = p.content,
                            orders = p.orders,
                            per_click = p.per_click,
                            start_date = _commonFunction.convertDateToStringSort(p.start_date),
                            end_date = _commonFunction.convertDateToStringSort(p.end_date),
                            user_created = p.user_created,
                            status = p.status
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(BannerRequest request, string username)
        {
            if (request.title == null)
            {
                return new APIResponse("ERROR_TITLE_MISSING");
            }

            var dataSame = _context.Banners.Where(x => x.title == request.title).FirstOrDefault();

            if (dataSame != null)
            {
                return new APIResponse("ERROR_TITLE_EXISTS");
            }

            if (request.start_date == null)
            {
                return new APIResponse("ERROR_START_DATE_MISSING");
            }
            if (request.end_date == null)
            {
                return new APIResponse("ERROR_END_DATE_MISSING");
            }
            if (request.image_link == null)
            {
                return new APIResponse("ERROR_IMAGE_LINK_MISSING");
            }
            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            try
            {
                var data = new Banner();
                data.title = request.title;
                data.title_url = request.title_url;
                data.image_link = request.image_link;
                data.orders = request.orders;
                data.content = request.content;
                data.per_click = 0;
                data.start_date = _commonFunction.convertStringSortToDate(request.start_date);
                data.end_date = _commonFunction.convertStringSortToDate(request.end_date);
                data.status = request.status;
                data.user_created = username;
                data.date_created = DateTime.Now;
                data.user_updated = username;
                data.date_updated = DateTime.Now;
                _context.Banners.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(BannerRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Banners.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.title == null)
            {
                return new APIResponse("ERROR_TITLE_MISSING");
            }
            if (request.start_date == null)
            {
                return new APIResponse("ERROR_START_DATE_MISSING");
            }
            if (request.end_date == null)
            {
                return new APIResponse("ERROR_END_DATE_MISSING");
            }
            if (request.image_link == null)
            {
                return new APIResponse("ERROR_IMAGE_LINK_MISSING");
            }
            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            try
            {
                data.title = request.title;
                data.title_url = request.title_url;
                data.image_link = request.image_link;
                data.content = request.content;
                data.orders = request.orders;
                data.start_date = _commonFunction.convertStringSortToDate(request.start_date);
                data.end_date = _commonFunction.convertStringSortToDate(request.end_date);
                data.status = request.status;
                data.user_updated = username;
                data.date_updated = DateTime.Now;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteRequest req)
        {
            var data = _context.Banners.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.Banners.Remove(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }

            return new APIResponse(200);
        }
        public APIResponse changeStatus(DeleteRequest req)
        {
            var data = _context.Banners.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (req.status_id == null)
            {
                return new APIResponse("ERROR_STATUS_ID_MISSING");
            }

            if (data.status == req.status_id)
            {
                return new APIResponse("ERROR_STATUS_CANNOT_CHANGE");
            }

            try
            {
                data.status = req.status_id;
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
