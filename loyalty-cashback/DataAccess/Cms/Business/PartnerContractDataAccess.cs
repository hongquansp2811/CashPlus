using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using System.Data;
using Newtonsoft.Json;

namespace LOYALTY.DataAccess
{
    public class PartnerContractDataAccess : IPartnerContract
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public PartnerContractDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(PartnerContractRequest request)
        {
            updateContractExpire();
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
            var lstData = (from p in _context.PartnerContracts
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           join sto in _context.Partners on p.partner_id equals sto.id into stos
                           from sto in stos.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where (p.is_delete == null || p.is_delete != true)
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               service_type_id = p.service_type_id,
                               service_type_name = sv != null ? sv.name : "",
                               from_date_origin = p.from_date,
                               to_date_origin = p.to_date,
                               from_date = _commonFunction.convertDateToStringSort(p.from_date),
                               to_date = _commonFunction.convertDateToStringSort(p.to_date),
                               partner_code = sto.code,
                               partner_name = sto.name,
                               discount_rate = p.discount_rate,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.contract_name != null && request.contract_name.Length > 0)
            {
                lstData = lstData.Where(x => x.partner_code.Trim().ToLower().Contains(request.contract_name.Trim().ToLower()) || x.partner_name.Trim().ToLower().Contains(request.contract_name.Trim().ToLower()));
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.from_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.to_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
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
            updateContractExpire();
            var dateNow = DateTime.Now;
            var accumulateConfig = (from p in _context.AccumulatePointConfigs
                                    where p.code == null && p.from_date <= dateNow && p.to_date >= dateNow && p.contract_id == id && p.status == 23
                                    select new
                                    {
                                        id = p.id,
                                        from_date = p.from_date,
                                        to_date = p.to_date,
                                        status = p.status,
                                        description = p.description,
                                        list_items = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == p.id).ToList()
                                    }).FirstOrDefault();

            var accumulateConfigGENERAL = (from p in _context.AccumulatePointConfigs
                                           where p.code == "GENERAL"
                                           select new
                                           {
                                               id = p.id,
                                               from_date = p.from_date,
                                               to_date = p.to_date,
                                               status = p.status,
                                               description = p.description,
                                               list_items = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == p.id).ToList()
                                           }).FirstOrDefault();

            var data = (from p in _context.PartnerContracts
                        join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                        from sv in svs.DefaultIfEmpty()
                        join sto in _context.Partners on p.partner_id equals sto.id into stos
                        from sto in stos.DefaultIfEmpty()
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            service_type_id = p.service_type_id,
                            service_type_name = sv != null ? sv.name : "",
                            from_date_origin = p.from_date,
                            to_date_origin = p.to_date,
                            from_date = _commonFunction.convertDateToStringSort(p.from_date),
                            to_date = _commonFunction.convertDateToStringSort(p.to_date),
                            partner_code = sto.code,
                            partner_name = sto.name,
                            discount_rate = p.discount_rate,
                            contract_name = p.contract_name,
                            contract_no = p.contract_no,
                            sign_date = _commonFunction.convertDateToStringSort(p.sign_date),
                            contract_type_id = p.contract_type_id,
                            files = p.files,
                            status = p.status,
                            partner_id = p.partner_id,
                            contact_name = p.contact_name,
                            phone = p.phone,
                            support_person_id = p.support_person_id,
                            support_person_phone = p.support_person_phone,
                            description = p.description,
                            is_GENERAL = p.is_GENERAL,
                            accumulateConfig = accumulateConfig != null ? accumulateConfig : accumulateConfigGENERAL
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(PartnerContractRequest request, string username)
        {
            if (request.contract_name == null)
            {
                return new APIResponse("ERROR_CONTRACT_NAME_MISSING");
            }

            if (request.contract_no == null)
            {
                return new APIResponse("ERROR_CONTRACT_NO_MISSING");
            }

            if (request.sign_date == null)
            {
                return new APIResponse("ERROR_SIGN_DATE_MISSING");
            }

            if (request.from_date == null)
            {
                return new APIResponse("ERROR_FROM_DATE_MISSING");
            }

            if (request.to_date == null)
            {
                return new APIResponse("ERROR_TO_DATE_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.discount_rate == null)
            {
                return new APIResponse("ERROR_DISCOUNT_RATE_MISSING");
            }

            if (request.contact_name == null)
            {
                return new APIResponse("ERROR_CONTACT_NAME_MISSING");
            }

            if (request.phone == null)
            {
                return new APIResponse("ERROR_PHONE_MISSING");
            }

            var dataSameCode = _context.PartnerContracts.Where(x => x.contract_no.ToLower().Trim() == request.contract_no.ToLower().Trim()).FirstOrDefault();

            if (dataSameCode != null)
            {
                return new APIResponse("ERROR_PARTNER_SAME_CONTRACT_NO");
            }

            var dataSameEffect = _context.PartnerContracts.Where(x => x.partner_id == request.partner_id && x.status == 12 && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now).FirstOrDefault();

            if (dataSameEffect != null)
            {
                return new APIResponse("ERROR_PARTNER_EFFECTIVE");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Tạo cửa hàng
                var data = new PartnerContract();
                data.id = Guid.NewGuid();
                data.contract_no = request.contract_no;
                data.contract_name = request.contract_name;
                data.sign_date = _commonFunction.convertStringSortToDate(request.sign_date);
                data.from_date = _commonFunction.convertStringSortToDate(request.from_date);
                data.to_date = _commonFunction.convertStringSortToDate(request.to_date);
                data.contract_type_id = request.contract_type_id;
                data.files = request.files;
                data.status = request.status;
                data.partner_id = request.partner_id;
                data.service_type_id = request.service_type_id;
                data.discount_rate = request.discount_rate;
                data.contact_name = request.contact_name;
                data.phone = request.phone;
                data.description = request.description;
                data.is_GENERAL = request.is_GENERAL;
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.PartnerContracts.Add(data);
                _context.SaveChanges();

                //Nếu có cấu hình riêng thì thêm vào 
                if (data.is_GENERAL == false)
                {
                    if(request.accumulateConfig == null)
                    {
                        return new APIResponse("ERROR_AccumulateConfig_MISSING");
                    }
                    if (request.accumulateConfig.code == null)
                    {
                        if (request.accumulateConfig.from_date == null)
                        {
                            return new APIResponse("ERROR_FROM_DATE_MISSING");
                        }

                        if (request.accumulateConfig.to_date == null)
                        {
                            return new APIResponse("ERROR_TO_DATE_MISSING");
                        }

                        var dataSame = _context.AccumulatePointConfigs.Where(x => x.contract_id == data.id && x.status == 23).Select(x => x.id).FirstOrDefault();

                        if (dataSame != null)
                        {
                            return new APIResponse("ERROR_EXISTS_ACTIVE");
                        }
                    }
                    try
                    {
                        var AccumulatePointConfig = new AccumulatePointConfig();
                        AccumulatePointConfig.id = Guid.NewGuid();
                        AccumulatePointConfig.code = request.accumulateConfig.code;
                        if (request.from_date != null)
                        {
                            AccumulatePointConfig.from_date = _commonFunction.convertStringSortToDate(request.from_date);
                        }

                        if (request.to_date != null)
                        {
                            AccumulatePointConfig.to_date = _commonFunction.convertStringSortToDate(request.to_date);
                        }

                        AccumulatePointConfig.contract_id = data.id;
                        AccumulatePointConfig.partner_id = data.partner_id;
                        AccumulatePointConfig.service_type_id = data.service_type_id;
                        AccumulatePointConfig.description = request.accumulateConfig.description;
                        AccumulatePointConfig.discount_rate = request.accumulateConfig.discount_rate;
                        AccumulatePointConfig.status = request.accumulateConfig.status;
                        AccumulatePointConfig.active = request.accumulateConfig.active != null ? request.accumulateConfig.active : false;
                        AccumulatePointConfig.user_created = username;
                        AccumulatePointConfig.user_updated = username;
                        AccumulatePointConfig.date_created = DateTime.Now;
                        AccumulatePointConfig.date_updated = DateTime.Now;
                        _context.AccumulatePointConfigs.Add(AccumulatePointConfig);
                        _context.SaveChanges();

                        decimal customerExchangeRate = 0;
                        for (int i = 0; i < request.accumulateConfig.list_items.Count; i++)
                        {
                            if (request.accumulateConfig.list_items[i].name == "Khách hàng")
                            {
                                customerExchangeRate = (decimal)request.accumulateConfig.list_items[i].discount_rate;
                            }
                            var item = new AccumulatePointConfigDetail();
                            item.id = Guid.NewGuid();
                            item.accumulate_point_config_id = AccumulatePointConfig.id;
                            item.name = request.accumulateConfig.list_items[i].name;
                            item.allocation_name = request.accumulateConfig.list_items[i].allocation_name;
                            item.discount_rate = request.accumulateConfig.list_items[i].discount_rate;
                            item.description = request.accumulateConfig.list_items[i].description;
                            _context.AccumulatePointConfigDetails.Add(item);
                        }
                        _context.SaveChanges();

                        if (data.status == 23)
                        {
                            var contractObj = _context.PartnerContracts.Where(x => x.id == data.id).FirstOrDefault();

                            var partnerObj1 = _context.Partners.Where(x => x.id == contractObj.partner_id).FirstOrDefault();

                            if (partnerObj1 != null)
                            {
                                partnerObj1.customer_discount_rate = Math.Round(customerExchangeRate * (decimal)contractObj.discount_rate / 10) / 10;
                                _context.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return new APIResponse("ERROR_ADD_AccumulateConfig_FAIL");
                    }
                }

                // Nếu hoàn thành thì cập nhật chiết khấu cửa hàng và chiết khấu khách hàng
                if (data.status == 12)
                {
                    var dateNow = DateTime.Now;
                    decimal customerExchange = 0;
                    // Lấy cấu hình đổi điểm hiệu lực
                    var accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id && x.status == 23).FirstOrDefault();

                    // Nếu không có riêng thì lấy chung
                    if (accumulateConfig == null)
                    {
                        accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();
                    }

                    if (accumulateConfig != null)
                    {
                        var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfig.id).ToList();

                        if (listAccuDetails.Count == 0)
                        {
                            return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
                        }

                        for (int i = 0; i < listAccuDetails.Count; i++)
                        {
                            if (listAccuDetails[i].allocation_name == "Khách hàng")
                            {
                                customerExchange = (decimal)listAccuDetails[i].discount_rate;
                            }
                        }
                    }

                    partnerObj.discount_rate = request.discount_rate;
                    partnerObj.customer_discount_rate = request.discount_rate * (customerExchange / 100);
                    _context.SaveChanges();
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

        public APIResponse update(PartnerContractRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.PartnerContracts.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            // Cập nhật trạng thái hết hiệu lực
            if (data.status == 12 && request.status == 13)
            {
                // Tiếp tục
                data.status = request.status;
                _context.SaveChanges();
                return new APIResponse(200);
            }
            else if (data.status != 11)
            {
                return new APIResponse("ERROR_STATUS_NOT_UPDATE");
            }


            if (request.contract_name == null)
            {
                return new APIResponse("ERROR_CONTRACT_NAME_MISSING");
            }

            if (request.sign_date == null)
            {
                return new APIResponse("ERROR_SIGN_DATE_MISSING");
            }

            if (request.from_date == null)
            {
                return new APIResponse("ERROR_FROM_DATE_MISSING");
            }

            if (request.to_date == null)
            {
                return new APIResponse("ERROR_TO_DATE_MISSING");
            }

            if (request.status == null)
            {
                return new APIResponse("ERROR_STATUS_MISSING");
            }

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            if (request.discount_rate == null)
            {
                return new APIResponse("ERROR_DISCOUNT_RATE_MISSING");
            }

            if (request.contact_name == null)
            {
                return new APIResponse("ERROR_CONTACT_NAME_MISSING");
            }

            if (request.phone == null)
            {
                return new APIResponse("ERROR_PHONE_MISSING");
            }

            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();

            var transaction = _context.Database.BeginTransaction();
            try
            {
                data.contract_name = request.contract_name;
                data.sign_date = _commonFunction.convertStringSortToDate(request.sign_date);
                data.from_date = _commonFunction.convertStringSortToDate(request.from_date);
                data.to_date = _commonFunction.convertStringSortToDate(request.to_date);
                data.contract_type_id = request.contract_type_id;
                data.files = request.files;
                data.status = request.status;
                data.partner_id = request.partner_id;
                data.discount_rate = request.discount_rate;
                data.contact_name = request.contact_name;
                data.phone = request.phone;
                data.description = request.description;
                data.is_GENERAL = request.is_GENERAL;
                _context.SaveChanges();
                var dataAccumulatePointConfig = new AccumulatePointConfig();
                if(data.is_GENERAL == false)
                {
                    dataAccumulatePointConfig = _context.AccumulatePointConfigs.Where(x => x.id == request.accumulateConfig.id).FirstOrDefault();
                }

                if (dataAccumulatePointConfig.id != null) 
                {
                    //Nếu có cấu hình riêng thì thêm vào 
                    if (request.accumulateConfig.code == null)
                    {
                        if (request.accumulateConfig.from_date == null)
                        {
                            return new APIResponse("ERROR_FROM_DATE_MISSING");
                        }

                        if (request.accumulateConfig.to_date == null)
                        {
                            return new APIResponse("ERROR_TO_DATE_MISSING");
                        }

                        if (request.accumulateConfig.contract_id == null)
                        {
                            return new APIResponse("ERROR_CONTRACT_ID_MISSING");
                        }

                        if (request.accumulateConfig.partner_id == null)
                        {
                            return new APIResponse("ERROR_PARTNER_ID_MISSING");
                        }

                        if (request.accumulateConfig.service_type_id == null)
                        {
                            return new APIResponse("ERROR_SERVICE_TYPE_ID_MISSING");
                        }

                        if (dataAccumulatePointConfig.status == 23)
                        {
                            if (request.accumulateConfig.status == 24)
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

                        var dataSame = _context.AccumulatePointConfigs.Where(x => x.contract_id == data.id && x.status == 23 && x.id != request.accumulateConfig.id).Select(x => x.id).FirstOrDefault();

                        if (dataSame != null)
                        {
                            return new APIResponse("ERROR_EXISTS_ACTIVE");
                        }
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

                        dataAccumulatePointConfig.contract_id = data.id;
                        dataAccumulatePointConfig.partner_id = data.partner_id;
                        dataAccumulatePointConfig.service_type_id = data.service_type_id;
                        dataAccumulatePointConfig.discount_rate = request.accumulateConfig.discount_rate;
                        dataAccumulatePointConfig.description = request.accumulateConfig.description;
                        dataAccumulatePointConfig.status = request.accumulateConfig.status;
                        dataAccumulatePointConfig.active = request.accumulateConfig.active != null ? request.accumulateConfig.active : false;
                        _context.SaveChanges();

                        var lstDeletes = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == dataAccumulatePointConfig.id).ToList();
                        _context.AccumulatePointConfigDetails.RemoveRange(lstDeletes);

                        decimal customerExchangeRate = 0;
                        if (dataAccumulatePointConfig != null && request.accumulateConfig.list_items != null && request.accumulateConfig.list_items.Count > 0)
                        {
                            for (int i = 0; i < request.accumulateConfig.list_items.Count; i++)
                            {
                                if (request.accumulateConfig.list_items[i].name == "Khách hàng")
                                {
                                    customerExchangeRate = (decimal)request.accumulateConfig.list_items[i].discount_rate;
                                }

                                var item = new AccumulatePointConfigDetail();
                                item.id = Guid.NewGuid();
                                item.accumulate_point_config_id = data.id;
                                item.name = request.accumulateConfig.list_items[i].name;
                                item.allocation_name = request.accumulateConfig.list_items[i].allocation_name;
                                item.discount_rate = request.accumulateConfig.list_items[i].discount_rate;
                                item.description = request.accumulateConfig.list_items[i].description;
                                _context.AccumulatePointConfigDetails.Add(item);
                            }
                        }

                        _context.SaveChanges();

                        if (data.status == 23)
                        {
                            if (data.id != null)
                            {
                                var contractObj = _context.PartnerContracts.Where(x => x.id == data.id).FirstOrDefault();

                                var partnerObj1 = _context.Partners.Where(x => x.id == contractObj.partner_id).FirstOrDefault();

                                if (partnerObj1 != null)
                                {
                                    partnerObj1.customer_discount_rate = Math.Round(customerExchangeRate * (decimal)contractObj.discount_rate / 10) / 10;

                                    _context.SaveChanges();
                                }
                            }
                        }

                        if (request.accumulateConfig.code != null && request.accumulateConfig.code == "GENERAL")
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
                        return new APIResponse("ERROR_UPDATE_AccumulatePointConfig_FAIL");
                    }
                }
                else if(dataAccumulatePointConfig == null && data.is_GENERAL == false && request.accumulateConfig != null)
                {
                    if (request.accumulateConfig.code == null)
                    {
                        if (request.accumulateConfig.from_date == null)
                        {
                            return new APIResponse("ERROR_FROM_DATE_MISSING");
                        }

                        if (request.accumulateConfig.to_date == null)
                        {
                            return new APIResponse("ERROR_TO_DATE_MISSING");
                        }

                        var dataSame = _context.AccumulatePointConfigs.Where(x => x.contract_id == data.id && x.status == 23).Select(x => x.id).FirstOrDefault();

                        if (dataSame != null)
                        {
                            return new APIResponse("ERROR_EXISTS_ACTIVE");
                        }
                    }
                    try
                    {
                        var AccumulatePointConfig = new AccumulatePointConfig();
                        AccumulatePointConfig.id = Guid.NewGuid();
                        AccumulatePointConfig.code = request.accumulateConfig.code;
                        if (request.from_date != null)
                        {
                            AccumulatePointConfig.from_date = _commonFunction.convertStringSortToDate(request.from_date);
                        }

                        if (request.to_date != null)
                        {
                            AccumulatePointConfig.to_date = _commonFunction.convertStringSortToDate(request.to_date);
                        }

                        AccumulatePointConfig.contract_id = data.id;
                        AccumulatePointConfig.partner_id = data.partner_id;
                        AccumulatePointConfig.service_type_id = data.service_type_id;
                        AccumulatePointConfig.description = request.accumulateConfig.description;
                        AccumulatePointConfig.discount_rate = request.accumulateConfig.discount_rate;
                        AccumulatePointConfig.status = request.accumulateConfig.status;
                        AccumulatePointConfig.active = request.accumulateConfig.active != null ? request.accumulateConfig.active : false;
                        AccumulatePointConfig.user_created = username;
                        AccumulatePointConfig.user_updated = username;
                        AccumulatePointConfig.date_created = DateTime.Now;
                        AccumulatePointConfig.date_updated = DateTime.Now;
                        _context.AccumulatePointConfigs.Add(AccumulatePointConfig);
                        _context.SaveChanges();

                        decimal customerExchangeRate = 0;
                        for (int i = 0; i < request.accumulateConfig.list_items.Count; i++)
                        {
                            if (request.accumulateConfig.list_items[i].name == "Khách hàng")
                            {
                                customerExchangeRate = (decimal)request.accumulateConfig.list_items[i].discount_rate;
                            }
                            var item = new AccumulatePointConfigDetail();
                            item.id = Guid.NewGuid();
                            item.accumulate_point_config_id = AccumulatePointConfig.id;
                            item.name = request.accumulateConfig.list_items[i].name;
                            item.allocation_name = request.accumulateConfig.list_items[i].allocation_name;
                            item.discount_rate = request.accumulateConfig.list_items[i].discount_rate;
                            item.description = request.accumulateConfig.list_items[i].description;
                            _context.AccumulatePointConfigDetails.Add(item);
                        }
                        _context.SaveChanges();

                        if (data.status == 23)
                        {
                            var contractObj = _context.PartnerContracts.Where(x => x.id == data.id).FirstOrDefault();

                            var partnerObj1 = _context.Partners.Where(x => x.id == contractObj.partner_id).FirstOrDefault();

                            if (partnerObj1 != null)
                            {
                                partnerObj1.customer_discount_rate = Math.Round(customerExchangeRate * (decimal)contractObj.discount_rate / 10) / 10;
                                _context.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return new APIResponse("ERROR_ADD_AccumulatePointConfig_FAIL");
                    }
                }
             

                // Nếu hoàn thành thì cập nhật chiết khấu cửa hàng và chiết khấu khách hàng
                if (data.status == 12)
                {
                    var dateNow = DateTime.Now;
                    decimal customerExchange = 0;
                    // Lấy cấu hình đổi điểm hiệu lực
                    var accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == request.partner_id && x.status == 23).FirstOrDefault();

                    // Nếu không có riêng thì lấy chung
                    if (accumulateConfig == null)
                    {
                        accumulateConfig = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();
                    }

                    if (accumulateConfig != null)
                    {
                        var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfig.id).ToList();

                        if (listAccuDetails.Count == 0)
                        {
                            return new APIResponse("ERROR_ACCUMULATE_CONFIG_NOT_SETTING");
                        }

                        for (int i = 0; i < listAccuDetails.Count; i++)
                        {
                            if (listAccuDetails[i].allocation_name == "Khách hàng")
                            {
                                customerExchange = (decimal)listAccuDetails[i].discount_rate;
                            }
                        }
                    }

                    partnerObj.discount_rate = request.discount_rate;
                    partnerObj.customer_discount_rate = request.discount_rate * (customerExchange / 100);
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

        public APIResponse delete(DeleteGuidRequest request)
        {
            var data = _context.PartnerContracts.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.status != 11 && data.status != 13)
            {
                return new APIResponse("ERROR_STATUS_NOT_DELETE");
            }

            var transaction = _context.Database.BeginTransaction();

            try
            {
                data.is_delete = true;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return new APIResponse("ERROR_DELETE_FAIL");
            }

            transaction.Commit();
            transaction.Dispose();
            return new APIResponse(200);
        }

        public void updateContractExpire()
        {
            var lstContractExpire = _context.PartnerContracts.Where(x => x.to_date <= DateTime.Now && x.status == 12).ToList();

            try
            {
                if (lstContractExpire.Count > 0)
                {
                    for (int i = 0; i < lstContractExpire.Count; i++)
                    {
                        lstContractExpire[i].status = 13;
                    }

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}
