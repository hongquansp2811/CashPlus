using System;
using System.Linq;
using System.Collections.Generic;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;

namespace LOYALTY.DataAccess
{
    public class AppPartnerContractDataAccess : IAppPartnerContract
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppPartnerContractDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }


        public APIResponse getDetailPartner(Guid partner_id)
        {
            var data = (from p in _context.Partners
                        join u in _context.Users.Where(x => x.is_partner_admin == true) on p.id equals u.partner_id
                        join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                        from sv in svs.DefaultIfEmpty()
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == partner_id
                        select new
                        {
                            id = p.id,
                            service_type_id = p.service_type_id,
                            service_type_name = sv != null ? sv.name : "",
                            store_type_id = p.store_type_id,
                            code = p.code,
                            name = p.name,
                            phone = p.phone,
                            email = p.email,
                            avatar = p.avatar,
                            store_owner = p.store_owner,
                            address = p.address,
                            start_hour = p.start_hour,
                            end_hour = p.end_hour,
                            working_day = p.working_day,
                            tax_tncn = p.tax_tncn,
                            tax_code = p.tax_code,
                            description = p.description,
                            product_label_id = p.product_label_id,
                            store_account = u.username,
                            status = p.status,
                            status_name = st != null ? st.name : "",
                            province_id = p.province_id,
                            district_id = p.district_id,
                            ward_id = p.ward_id,
                            latitude = p.latitude,
                            longtitude = p.longtitude,
                            total_point = u.total_point,
                            point_waiting = u.point_waiting,
                            point_avaiable = u.point_avaiable,
                            point_affiliate = u.point_affiliate,
                            bk_partner_code = p.bk_partner_code,
                            bk_merchant_id = p.bk_merchant_id,
                            bk_email = p.bk_email,
                            bk_password = p.bk_password,
                            bk_bank_id = p.bk_bank_id,
                            bk_bank_no = p.bk_bank_no,
                            bk_bank_name = p.bk_bank_name,
                            bk_bank_owner = p.bk_bank_owner,
                            list_bank_accounts = _context.CustomerBankAccounts.Where(x => x.user_id == u.id).ToList(),
                            API_KEY = p.API_KEY,
                            API_SECRET = p.API_SECRET,
                            RSA_privateKey = p.RSA_privateKey,
                            RSA_publicKey = p.RSA_publicKey,
                            Encrypt_status = p.Encrypt_status != null ? p.Encrypt_status : 0,
                            link_QR = p.link_QR,
                            license_image = p.license_image,
                            list_documents = _context.PartnerDocuments.Where(x => x.partner_id == p.id).ToList(),
                            identifier_front_image = p.identifier_front_image,
                            identifier_back_image = p.identifier_back_image
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
            if (data == null)
            {
                return new APIResponse("ERROR_CONTRACT");
            }
            return new APIResponse(data);
        }

        public APIResponse getDetailPartnerContract(Guid partner_id)
        {
            var dateNow = DateTime.Now;
            var accumulateConfig = (from p in _context.AccumulatePointConfigs
                      where p.code == null && p.from_date <= dateNow && p.to_date >= dateNow && p.partner_id == partner_id && p.status == 23
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
                        join ct in _context.OtherLists on p.contract_type_id equals ct.id into cts
                        from ct in cts.DefaultIfEmpty()
                        join sp in _context.Users on p.support_person_id equals sp.id into sps
                        from sp in sps.DefaultIfEmpty()
                        where p.partner_id == partner_id && p.status == 12 && p.from_date <= dateNow && p.to_date >= dateNow
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
                            contract_type_name = ct != null ? ct.name : "",
                            files = p.files,
                            status = p.status,
                            partner_id = p.partner_id,
                            contact_name = p.contact_name,
                            phone = p.phone,
                            support_person_id = p.support_person_id,
                            support_peron_name = sp != null ? sp.full_name : "",
                            support_person_phone = p.support_person_phone,
                            description = p.description,
                            is_GENERAL = p.is_GENERAL,
                            accumulateConfig = accumulateConfig != null ? accumulateConfig : accumulateConfigGENERAL
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_CONTRACT");
            }
            return new APIResponse(data);
        }
    }
}
