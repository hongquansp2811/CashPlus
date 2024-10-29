using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using Newtonsoft.Json;

namespace LOYALTY.DataAccess
{
    public class AffiliateConfigDataAccess : IAffiliateConfig
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AffiliateConfigDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AffiliateConfigRequest request)
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
            var lstData = (from p in _context.AffiliateConfigs
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           where p.code == null
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
                               description = p.description
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
            var data = (from p in _context.AffiliateConfigs
                        join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                        from sv in svs.DefaultIfEmpty()
                        where p.code == null && p.id == id
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
                            date_return = p.date_return,
                            hours_return = _commonFunction.convertTimeSpanToStringSort(p.hours_return),
                            list_items = _context.AffiliateConfigDetails.Where(x => x.affiliate_config_id == p.id).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getDetailGeneral()
        {
            var data = (from p in _context.AffiliateConfigs
                        where p.code == "GENERAL"
                        select new
                        {
                            id = p.id,
                            code = p.code,
                            date_return = p.date_return,
                            hours_return = p.hours_return != null ? _commonFunction.convertTimeSpanToStringSort(p.hours_return) : "",
                            list_items = _context.AffiliateConfigDetails.Where(x => x.affiliate_config_id == p.id).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(AffiliateConfigRequest request, string username)
        {
            if (request.code == null)
            {
                if (request.from_date == null)
                {
                    return new APIResponse("ERROR_FROM_DATE_MISSING");
                }

                if (request.to_date == null)
                {
                    return new APIResponse("ERROR_TO_DATE_MISSING");
                }
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                var data = new AffiliateConfig();
                data.id = Guid.NewGuid();
                data.code = request.code;
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
                data.date_return = request.date_return;
                if (request.hours_return != null)
                {
                    data.hours_return = _commonFunction.convertStringSortToTimeSpan(request.hours_return);
                }
                data.active = request.active != null ? request.active : false;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.AffiliateConfigs.Add(data);
                _context.SaveChanges();

                //for (int i = 0; i < request.list_items.Count; i++)
                //{
                //    var item = new AffiliateConfigDetail();
                //    item.id = Guid.NewGuid();
                //    item.affiliate_config_id = data.id;
                //    item.levels = request.list_items[i].levels;
                //    item.discount_rate = request.list_items[i].discount_rate;
                //    _context.AffiliateConfigDetails.Add(item);
                //}

                //_context.SaveChanges();
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

        public APIResponse update(AffiliateConfigRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.AffiliateConfigs.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.code == null)
            {
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
            }

            var transaction = _context.Database.BeginTransaction();

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
                data.active = request.active != null ? request.active : false;
                data.date_return = request.date_return;
                if (request.hours_return != null)
                {
                    data.hours_return = _commonFunction.convertStringSortToTimeSpan(request.hours_return);
                }

                _context.SaveChanges();

                //var lstDeletes = _context.AffiliateConfigDetails.Where(x => x.affiliate_config_id == data.id).ToList();
                //_context.AffiliateConfigDetails.RemoveRange(lstDeletes);

                //if (data != null && request.list_items != null && request.list_items.Count > 0)
                //{
                //    for (int i = 0; i < request.list_items.Count; i++)
                //    {
                //        var item = new AffiliateConfigDetail();
                //        item.id = Guid.NewGuid();
                //        item.affiliate_config_id = data.id;
                //        item.levels = request.list_items[i].levels;
                //        item.discount_rate = request.list_items[i].discount_rate;
                //        _context.AffiliateConfigDetails.Add(item);
                //    }
                //}

                //_context.SaveChanges();

                if (request.code != null && request.code == "GENERAL")
                {
                    var newHistory = new ConfigHistory();
                    newHistory.id = Guid.NewGuid();
                    newHistory.config_type = "AFFILIATE_CONFIG";
                    newHistory.date_created = DateTime.Now;
                    newHistory.user_created = username;
                    newHistory.data_logging = JsonConvert.SerializeObject(request);
                    _context.ConfigHistorys.Add(newHistory);
                    _context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_UPDATE_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }

        public APIResponse delete(DeleteGuidRequest req)
        {
            var data = _context.AffiliateConfigs.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.active == true || data.code != null)
            {
                return new APIResponse("ERROR_STATUS_NOT_DELETE");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                var lstDeletes = _context.AffiliateConfigDetails.Where(x => x.affiliate_config_id == data.id).ToList();
                _context.AffiliateConfigDetails.RemoveRange(lstDeletes);
                _context.SaveChanges();

                _context.AffiliateConfigs.Remove(data);
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
