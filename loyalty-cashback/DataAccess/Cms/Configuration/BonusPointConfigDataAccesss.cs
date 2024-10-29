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
    public class BonusPointConfigDataAccess : IBonusPointConfig
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public BonusPointConfigDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(BonusPointConfigRequest request)
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
            var lstData = (from p in _context.BonusPointConfigs
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               from_date_origin = p.from_date,
                               to_date_origin = p.to_date,
                               from_date = _commonFunction.convertDateToStringSort(p.from_date),
                               to_date = _commonFunction.convertDateToStringSort(p.to_date),
                               active = p.active,
                               service_type_id = p.service_type_id,
                               service_type_name = sv.name,
                               description = p.description,
                               min_point = p.min_point,
                               max_point = p.max_point,
                               discount_rate = p.discount_rate
                           });

            // Nếu tồn tại Where theo tên
            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
            }

            if (request.active != null)
            {
                lstData = lstData.Where(x => x.active == request.active);
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

        public APIResponse getDetail(Guid id)
        {
            var data = (from p in _context.BonusPointConfigs
                        join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                        from sv in svs.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            from_date_origin = p.from_date,
                            to_date_origin = p.to_date,
                            from_date = _commonFunction.convertDateToStringSort(p.from_date),
                            to_date = _commonFunction.convertDateToStringSort(p.to_date),
                            active = p.active,
                            service_type_id = p.service_type_id,
                            service_type_name = sv.name,
                            description = p.description,
                            min_point = p.min_point,
                            max_point = p.max_point,
                            discount_rate = p.discount_rate
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(BonusPointConfigRequest request, string username)
        {

            if (request.from_date == null)
            {
                return new APIResponse("ERROR_FROM_DATE_MISSING");
            }

            if (request.to_date == null)
            {
                return new APIResponse("ERROR_TO_DATE_MISSING");
            }

            if (request.discount_rate == null)
            {
                return new APIResponse("ERROR_DISCOUNT_RATE_MISSING");
            }

            if (request.min_point == null)
            {
                return new APIResponse("ERROR_MIN_POINT_MISSING");
            }

            if (request.max_point == null)
            {
                return new APIResponse("ERROR_MAX_POINT_MISSING");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                var data = new BonusPointConfig();
                data.id = Guid.NewGuid();
                if (request.from_date != null)
                {
                    data.from_date = _commonFunction.convertStringSortToDate(request.from_date);
                }

                if (request.to_date != null)
                {
                    data.to_date = _commonFunction.convertStringSortToDate(request.to_date);
                }

                data.service_type_id = request.service_type_id;
                data.description = request.description;
                data.discount_rate = request.discount_rate;
                data.min_point = request.min_point;
                data.max_point = request.max_point;
                data.active = request.active != null ? request.active : false;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.BonusPointConfigs.Add(data);
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

        public APIResponse update(BonusPointConfigRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.BonusPointConfigs.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.active == true)
            {
                return new APIResponse("ERROR_STATUS_NOT_UPDATE");
            }

            if (request.from_date == null)
            {
                return new APIResponse("ERROR_FROM_DATE_MISSING");
            }

            if (request.to_date == null)
            {
                return new APIResponse("ERROR_TO_DATE_MISSING");
            }

            if (data.active == true)
            {
                return new APIResponse("ERROR_STATUS_NOT_DELETE");
            }

            try
            {
                if (request.from_date != null)
                {
                    data.from_date = _commonFunction.convertStringSortToDate(request.from_date);
                }

                if (request.to_date != null)
                {
                    data.to_date = _commonFunction.convertStringSortToDate(request.to_date);
                }

                data.service_type_id = request.service_type_id;
                data.description = request.description;
                data.discount_rate = request.discount_rate;
                data.min_point = request.min_point;
                data.max_point = request.max_point;
                data.active = request.active != null ? request.active : false;
                _context.SaveChanges();

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
            var data = _context.BonusPointConfigs.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.active == true)
            {
                return new APIResponse("ERROR_STATUS_NOT_DELETE");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.BonusPointConfigs.Remove(data);
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
    }
}
