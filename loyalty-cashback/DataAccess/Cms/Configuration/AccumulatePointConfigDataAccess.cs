using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using Newtonsoft.Json;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class AccumulatePointConfigDataAccess : IAccumulatePointConfig
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AccumulatePointConfigDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AccumulatePointConfigRequest request)
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
            var lstData = (from p in _context.AccumulatePointConfigs
                           join c in _context.PartnerContracts on p.contract_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join s in _context.Partners on p.partner_id equals s.id
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.code == null
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               from_date_origin = p.from_date,
                               to_date_origin = p.to_date,
                               from_date = _commonFunction.convertDateToStringSort(p.from_date),
                               to_date = _commonFunction.convertDateToStringSort(p.to_date),
                               contract_id = p.contract_id,
                               contract_no = c.contract_no,
                               partner_code = s.code,
                               partner_name = s.name,
                               active = p.active,
                               service_type_id = p.service_type_id,
                               service_type_name = sv.name,
                               discount_rate = p.discount_rate,
                               description = p.description,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.search != null && request.search.Length > 0)
            {
                lstData = lstData.Where(x => x.partner_code.Trim().ToLower().Contains(request.search.Trim().ToLower()) || x.partner_name.Trim().ToLower().Contains(request.search.Trim().ToLower()) || x.contract_no.Trim().ToLower().Contains(request.search.Trim().ToLower()));
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
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
            var data = (from p in _context.AccumulatePointConfigs
                        join c in _context.PartnerContracts on p.contract_id equals c.id into cs
                        from c in cs.DefaultIfEmpty()
                        join s in _context.Partners on p.partner_id equals s.id
                        join sv in _context.ServiceTypes on p.service_type_id equals sv.id
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            from_date_origin = p.from_date,
                            to_date_origin = p.to_date,
                            from_date = _commonFunction.convertDateToStringSort(p.from_date),
                            to_date = _commonFunction.convertDateToStringSort(p.to_date),
                            contract_id = p.contract_id,
                            contract_no = c.contract_no,
                            partner_id = p.partner_id,
                            partner_code = s.code,
                            partner_name = s.name,
                            active = p.active,
                            service_type_id = p.service_type_id,
                            service_type_name = sv.name,
                            discount_rate = p.discount_rate,
                            description = p.description,
                            status = p.status,
                            status_name = st != null ? st.name : "",
                            list_items = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == p.id).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getDetailGeneral()
        {
            var data = (from p in _context.AccumulatePointConfigs
                        where p.code == "GENERAL"
                        select new
                        {
                            id = p.id,
                            list_items = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == p.id).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(AccumulatePointConfigRequest request, string username)
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

                if (request.contract_id == null)
                {
                    return new APIResponse("ERROR_CONTRACT_ID_MISSING");
                }

                if (request.partner_id == null)
                {
                    return new APIResponse("ERROR_PARTNER_ID_MISSING");
                }

                if (request.service_type_id == null)
                {
                    return new APIResponse("ERROR_SERVICE_TYPE_ID_MISSING");
                }

                var dataSame = _context.AccumulatePointConfigs.Where(x => x.contract_id == request.contract_id && x.status == 23).Select(x => x.id).FirstOrDefault();

                if (dataSame != null)
                {
                    return new APIResponse("ERROR_EXISTS_ACTIVE");
                }
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                var data = new AccumulatePointConfig();
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

                data.contract_id = request.contract_id;
                data.partner_id = request.partner_id;
                data.service_type_id = request.service_type_id;
                data.description = request.description;
                data.discount_rate = request.discount_rate;
                data.status = request.status;
                data.active = request.active != null ? request.active : false;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.AccumulatePointConfigs.Add(data);
                _context.SaveChanges();

                decimal customerExchangeRate = 0;
                for (int i = 0; i < request.list_items.Count; i++)
                {
                    if (request.list_items[i].name == "Khách hàng")
                    {
                        customerExchangeRate = (decimal)request.list_items[i].discount_rate;
                    }
                    var item = new AccumulatePointConfigDetail();
                    item.id = Guid.NewGuid();
                    item.accumulate_point_config_id = data.id;
                    item.name = request.list_items[i].name;
                    item.allocation_name = request.list_items[i].allocation_name;
                    item.discount_rate = request.list_items[i].discount_rate;
                    item.description = request.list_items[i].description;
                    _context.AccumulatePointConfigDetails.Add(item);
                }

                _context.SaveChanges();

                if (data.status == 23)
                {
                    if (request.contract_id != null)
                    {
                        var contractObj = _context.PartnerContracts.Where(x => x.id == request.contract_id).FirstOrDefault();

                        var partnerObj = _context.Partners.Where(x => x.id == contractObj.partner_id).FirstOrDefault();

                        if (partnerObj != null)
                        {
                            partnerObj.customer_discount_rate = Math.Round(customerExchangeRate * (decimal)contractObj.discount_rate / 10) / 10;

                            _context.SaveChanges();
                        }
                    }
                }
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

        public APIResponse update(AccumulatePointConfigRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.AccumulatePointConfigs.Where(x => x.id == request.id).FirstOrDefault();
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

                if (request.contract_id == null)
                {
                    return new APIResponse("ERROR_CONTRACT_ID_MISSING");
                }

                if (request.partner_id == null)
                {
                    return new APIResponse("ERROR_PARTNER_ID_MISSING");
                }

                if (request.service_type_id == null)
                {
                    return new APIResponse("ERROR_SERVICE_TYPE_ID_MISSING");
                }

                if (data.status == 23)
                {
                    if (request.status == 24)
                    {
                        data.status = 24;
                        _context.SaveChanges();
                        return new APIResponse(200);
                    }
                    else
                    {
                        return new APIResponse("ERROR_STATUS_NOT_UPDATE");
                    }
                }

                var dataSame = _context.AccumulatePointConfigs.Where(x => x.contract_id == request.contract_id && x.status == 23 && x.id != request.id).Select(x => x.id).FirstOrDefault();

                if (dataSame != null)
                {
                    return new APIResponse("ERROR_EXISTS_ACTIVE");
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

                data.contract_id = request.contract_id;
                data.partner_id = request.partner_id;
                data.service_type_id = request.service_type_id;
                data.discount_rate = request.discount_rate;
                data.description = request.description;
                data.status = request.status;
                data.active = request.active != null ? request.active : false;
                _context.SaveChanges();

                var lstDeletes = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == data.id).ToList();
                _context.AccumulatePointConfigDetails.RemoveRange(lstDeletes);

                decimal customerExchangeRate = 0;
                if (data != null && request.list_items != null && request.list_items.Count > 0)
                {
                    for (int i = 0; i < request.list_items.Count; i++)
                    {
                        if (request.list_items[i].name == "Khách hàng")
                        {
                            customerExchangeRate = (decimal)request.list_items[i].discount_rate;
                        }

                        var item = new AccumulatePointConfigDetail();
                        item.id = Guid.NewGuid();
                        item.accumulate_point_config_id = data.id;
                        item.name = request.list_items[i].name;
                        item.allocation_name = request.list_items[i].allocation_name;
                        item.discount_rate = request.list_items[i].discount_rate;
                        item.description = request.list_items[i].description;
                        _context.AccumulatePointConfigDetails.Add(item);
                    }
                }

                _context.SaveChanges();

                if (data.status == 23)
                {
                    if (request.contract_id != null)
                    {
                        var contractObj = _context.PartnerContracts.Where(x => x.id == request.contract_id).FirstOrDefault();

                        var partnerObj = _context.Partners.Where(x => x.id == contractObj.partner_id).FirstOrDefault();

                        if (partnerObj != null)
                        {
                            partnerObj.customer_discount_rate = Math.Round(customerExchangeRate * (decimal)contractObj.discount_rate / 10) / 10;

                            _context.SaveChanges();
                        }
                    }
                }

                if (request.code != null && request.code == "GENERAL")
                {
                    var newHistory = new ConfigHistory();
                    newHistory.id = Guid.NewGuid();
                    newHistory.config_type = "ACCUMULATE_POINT_CONFIG";
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
            var data = _context.AccumulatePointConfigs.Where(x => x.id == req.id).FirstOrDefault();
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
                var lstDeletes = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == data.id).ToList();
                _context.AccumulatePointConfigDetails.RemoveRange(lstDeletes);
                _context.SaveChanges();

                _context.AccumulatePointConfigs.Remove(data);
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
