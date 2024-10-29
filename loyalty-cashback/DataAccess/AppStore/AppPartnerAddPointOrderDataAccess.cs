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
    public class AppAddPointOrderDataAccess : IPartnerAddPointOrder
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppAddPointOrderDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(AddPointOrderRequest request)
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

            var lstData = (from p in _context.AddPointOrders
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                               trans_date_origin = p.date_created,
                               point_exchange = p.point_exchange,
                               bill_amount = p.bill_amount,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.from_date != null && request.from_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date));
            }

            if (request.to_date != null && request.to_date.Length == 10)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date));
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
            var data = (from p in _context.AddPointOrders
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            trans_no = p.trans_no,
                            trans_date = _commonFunction.convertDateToStringFull(p.date_created),
                            bill_amount = p.bill_amount,
                            point_exchange = p.point_exchange,
                            status = p.status,
                            status_name = st != null ? st.name : ""
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getCustomerFakeBank(Guid partner_id)
        {
            var data = (from p in _context.CustomerFakeBanks
                        where p.user_id == partner_id
                        select p).FirstOrDefault();

            if (data == null)
            {
                return new APIResponse("ERROR_CUSTOMER_FAKE_BANK_NOT_FOUND");
            }
            return new APIResponse(data);
        }

        public APIResponse getBonusPointDescription(Guid partner_id)
        {
            var partnerObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (partnerObj == null)
            {
                return new APIResponse("ERROR_PARTNER_NOT_FOUND");
            }

            var bonusConfigServiceTypeObj = _context.BonusPointConfigs.Where(x => x.service_type_id == partnerObj.service_type_id && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.active == true).FirstOrDefault();

            if (bonusConfigServiceTypeObj == null)
            {
                bonusConfigServiceTypeObj = _context.BonusPointConfigs.Where(x => x.service_type_id == null && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.active == true).FirstOrDefault();
            }

            string description = "";
            if (bonusConfigServiceTypeObj != null)
            {
                description = bonusConfigServiceTypeObj.description;
            }
            var data = new
            {
                bonus_description = description
            };
            return new APIResponse(data);
        }
    }
}
