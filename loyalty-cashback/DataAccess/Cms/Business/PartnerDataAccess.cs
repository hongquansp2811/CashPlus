using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.PaymentGate;
using System.Threading.Tasks;
using LOYALTY.PaymentGate.Interface;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Ocsp;

namespace LOYALTY.DataAccess
{
    public class PartnerDataAccess : IPartner
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        private readonly IEmailSender _emailSender;
        private readonly BKTransaction _bkTransaction;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISendSMSBrandName _sendSMSBrandName;
        public PartnerDataAccess(LOYALTYContext context, ICommonFunction commonFunction, IEmailSender emailSender, BKTransaction bKTransaction, IServiceScopeFactory serviceScopeFactory, ISendSMSBrandName sendSMSBrandName)
        {
            this._context = context;
            _commonFunction = commonFunction;
            _emailSender = emailSender;
            _bkTransaction = bKTransaction;
            _sendSMSBrandName = sendSMSBrandName;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public APIResponse getList(PartnerRequest request)
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
            var lstData = (from p in _context.Partners
                           join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                           from sv in svs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.is_delete != true
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               service_type_id = p.service_type_id,
                               service_type_name = sv != null ? sv.name : "",
                               code = p.code,
                               name = p.name,
                               phone = p.phone,
                               store_owner = p.store_owner,
                               address = p.address,
                               status = p.status,
                               status_name = st != null ? st.name : "",
                               province_id = p.province_id,
                               district_id = p.district_id,
                               ward_id = p.ward_id,
                               id_contract = _context.PartnerContracts.Where(l => l.partner_id == p.id && l.status == 12).Select(l => l.id).FirstOrDefault()
                           });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstData = lstData.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.phone.Trim().ToLower().Contains(request.name.Trim().ToLower())
                || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
            }

            if (request.status != null)
            {
                lstData = lstData.Where(x => x.status == request.status);
            }

            if (request.province_id != null)
            {
                lstData = lstData.Where(x => x.province_id == request.province_id);
            }

            if (request.district_id != null)
            {
                lstData = lstData.Where(x => x.district_id == request.district_id);
            }

            if (request.ward_id != null)
            {
                lstData = lstData.Where(x => x.ward_id == request.ward_id);
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
            var data = (from p in _context.Partners
                        join u in _context.Users on p.id equals u.partner_id
                        join sv in _context.ServiceTypes on p.service_type_id equals sv.id into svs
                        from sv in svs.DefaultIfEmpty()
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id && u.is_partner_admin == true
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
                            store_owner = p.store_owner,
                            address = p.address,
                            start_hour = p.start_hour,
                            end_hour = p.end_hour,
                            working_day = p.working_day,
                            tax_tncn = p.tax_tncn,
                            tax_code = p.tax_code,
                            description = p.description,
                            product_label_id = p.product_label_id,
                            avatar = p.avatar,
                            username = u.username,
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
                            license_image = p.license_image,
                            license_no = p.license_no,
                            license_person_number = p.license_person_number,
                            license_owner = p.license_owner,
                            license_date = p.license_date != null ? _commonFunction.convertDateToStringSort(p.license_date) : "",
                            license_birth_date = p.license_birth_date != null ? _commonFunction.convertDateToStringSort(p.license_birth_date) : "",
                            license_nation_id = p.license_nation_id,
                            indetifier_no = p.indetifier_no,
                            identifier_date = p.identifier_date != null ? _commonFunction.convertDateToStringSort(p.identifier_date) : "",
                            identifier_at = p.identifier_at,
                            identifier_date_expire = p.identifier_date_expire != null ? _commonFunction.convertDateToStringSort(p.identifier_date_expire) : "",
                            identifier_address = p.identifier_address,
                            identifier_nation_id = p.identifier_nation_id,
                            identifier_province_id = p.identifier_province_id,
                            is_same_address = p.is_same_address,
                            now_address = p.now_address,
                            now_nation_id = p.now_nation_id,
                            now_province_id = p.now_province_id,
                            identifier_front_image = p.identifier_front_image,
                            identifier_back_image = p.identifier_back_image,
                            discount_rate = p.discount_rate,
                            support_person_id = p.support_person_id,
                            support_person_phone = p.support_person_phone,
                            link_share = Consts.LINK_SHARE + u.share_code,
                            bk_partner_code = p.bk_partner_code,
                            bk_merchant_id = p.bk_merchant_id,
                            bk_email = p.bk_email,
                            bk_password = p.bk_password,
                            bk_bank_id = p.bk_bank_id,
                            bk_bank_no = p.bk_bank_no,
                            bk_bank_owner = p.bk_bank_owner,
                            bk_bank_name = p.bk_bank_name,
                            // list_bank_accounts = _context.CustomerBankAccounts.Where(x => x.user_id == p.id).ToList(),
                            list_bank_accounts = (from cb in _context.CustomerBankAccounts
                                            join b in _context.Banks on cb.bank_id equals b.id
                                             where cb.user_id == p.id
                                             select new
                                             {
                                                 id = cb.id,
                                                 bank_id = cb.bank_id,
                                                 bank_name = b.name,
                                                 bank_no = cb.bank_no,
                                                 bank_owner = cb.bank_owner
                                             }).ToList(),
                            list_documents = _context.PartnerDocuments.Where(x => x.partner_id == p.id).ToList(),
                            API_KEY = p.API_KEY,
                            API_SECRET = p.API_SECRET,
                            RSA_publicKey = p.RSA_publicKey,
                            RSA_privateKey = p.RSA_privateKey,
                            Encrypt_status = p.Encrypt_status != null ? p.Encrypt_status : 0,
                            link_QR = p.link_QR
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse getBalance(Guid partner_id)
        {
            decimal amount_balance = 0;
            var partnerObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (partnerObj != null && partnerObj.bk_partner_code != null) 
            {
             
                    GetBalanceResponseObj balanceObj = _bkTransaction.getBalanceFirmBank(partnerObj.bk_partner_code, partnerObj.RSA_privateKey);
                    amount_balance = balanceObj.Available;
            }

            return new APIResponse(new
            {
                amount_balance = amount_balance
            });
        }
        public APIResponse create(PartnerRequest request, string username)
        {
            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.service_type_id == null)
            {
                return new APIResponse("ERROR_SERVICE_TYPE_ID_MISSING");
            }

            if (request.store_type_id == null)
            {
                return new APIResponse("ERROR_STORE_TYPE_ID_MISSING");
            }

            if (request.phone == null)
            {
                return new APIResponse("ERROR_PHONE_MISSING");
            }

            if (request.start_hour == null)
            {
                return new APIResponse("ERROR_START_HOUR_MISSING");
            }

            if (request.end_hour == null)
            {
                return new APIResponse("ERROR_END_HOUR_MISSING");
            }

            var dataSameUsername = _context.Users.Where(x => x.username.ToLower().Trim() == request.username.ToLower().Trim() && x.is_partner == true).FirstOrDefault();

            if (dataSameUsername != null)
            {
                return new APIResponse("ERROR_PARTNER_SAME_USERNAME");
            }
            if (request.bk_partner_code != null && request.Encrypt_status == 0)
            {
                return new APIResponse("ERROR_Encrypt");
            }

            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Tạo cửa hàng
                var data = new Partner();
                data.id = Guid.NewGuid();
                var serviceTypeObj = _context.ServiceTypes.Where(x => x.id == request.service_type_id).FirstOrDefault();

                string serviceTypeCode = (serviceTypeObj != null && serviceTypeObj.code != null) ? serviceTypeObj.code : "LDV";
                var maxCodeObject = _context.Partners.Where(x => x.code != null && x.code.Contains(serviceTypeCode)).OrderByDescending(x => x.code).FirstOrDefault();
                string code = "";
                if (maxCodeObject == null)
                {
                    code = serviceTypeCode + "00000001";
                }
                else
                {
                    string maxCode = maxCodeObject.code;
                    maxCode = maxCode.Substring(serviceTypeCode.Length);
                    int orders = 1;
                    try
                    {
                        orders = int.Parse(maxCode);
                    }
                    catch
                    {

                    }
                    orders = orders + 1;
                    string orderString = orders.ToString();
                    char pad = '0';
                    int number = 8;
                    code = serviceTypeCode + orderString.PadLeft(number, pad);
                }

                data.code = code;
                data.name = request.name;
                data.service_type_id = request.service_type_id;
                data.store_type_id = request.store_type_id;
                data.phone = request.phone;
                data.email = request.email;
                data.avatar = request.avatar;
                data.store_owner = request.store_owner;
                data.username = request.username;
                data.start_hour = request.start_hour;
                data.end_hour = request.end_hour;
                data.working_day = request.working_day;
                data.tax_tncn = request.tax_tncn;
                data.tax_code = request.tax_code;
                data.description = request.description;
                data.product_label_id = request.product_label_id;
                data.province_id = request.province_id;
                data.district_id = request.district_id;
                data.ward_id = request.ward_id;
                data.address = request.address;
                data.latitude = request.latitude;
                data.longtitude = request.longtitude;
                data.status = 15; // Check Trạng thái
                data.total_rating = 0;
                data.rating = 0;
                data.discount_rate = request.discount_rate;
                data.support_person_id = request.support_person_id;
                data.support_person_phone = request.support_person_phone;
                data.is_delete = false;

                // Bổ sung 14/09
                data.license_image = request.license_image;
                data.license_no = request.license_no;
                data.license_person_number = request.license_person_number;
                data.license_owner = request.license_owner;
                if (request.license_date != null)
                {
                    data.license_date = _commonFunction.convertStringSortToDate(request.license_date);
                }
                if (request.license_birth_date != null)
                {
                    data.license_birth_date = _commonFunction.convertStringSortToDate(request.license_birth_date);
                }

                if (request.identifier_date != null)
                {
                    data.identifier_date = _commonFunction.convertStringSortToDate(request.identifier_date);
                }

                if (request.identifier_date_expire != null)
                {
                    data.identifier_date_expire = _commonFunction.convertStringSortToDate(request.identifier_date_expire);
                }
                data.license_nation_id = request.license_nation_id;
                data.indetifier_no = request.indetifier_no;
                data.identifier_at = request.identifier_at;
                data.identifier_address = request.identifier_address;
                data.identifier_province_id = request.identifier_province_id;
                data.is_same_address = request.is_same_address != null ? request.is_same_address : false;
                data.now_address = request.now_address;
                data.now_nation_id = request.now_nation_id;
                data.now_province_id = request.now_province_id;
                data.identifier_front_image = request.identifier_front_image;
                data.identifier_back_image = request.identifier_back_image;
                data.owner_percent = request.owner_percent;
                data.identifier_nation_id = request.identifier_nation_id;

                data.bk_partner_code = request.bk_partner_code;
                data.bk_merchant_id =  request.bk_merchant_id ;
                data.bk_email = request.bk_email;
                data.bk_password = request.bk_password;
                data.bk_bank_id = request.bk_bank_id;
                data.bk_bank_no = request.bk_bank_no;
                data.bk_bank_owner = request.bk_bank_owner;
                data.bk_bank_name = request.bk_bank_name;
                data.API_KEY =  request.API_KEY ;
                data.API_SECRET = request.API_SECRET ;
                data.Encrypt_status = request.Encrypt_status;
                data.RSA_privateKey = RSASign.rplRSAPublicKey(request.RSA_privateKey);
                data.RSA_publicKey = RSASign.rplRSAPublicKey(request.RSA_publicKey);
                data.link_QR = request.link_QR;
                 
                data.user_created = username;
                data.user_updated = username;
                data.date_created = DateTime.Now;
                data.date_updated = DateTime.Now;
                _context.Partners.Add(data);
                _context.SaveChanges();

                // Tạo tài liệu
                if (request.list_documents != null && request.list_documents.Count > 0)
                {
                    for (int i = 0; i < request.list_documents.Count; i++)
                    {
                        var item = new PartnerDocument();
                        item.id = Guid.NewGuid();
                        item.partner_id = data.id;
                        item.file_name = request.list_documents[i].file_name;
                        item.links = request.list_documents[i].links;
                        _context.PartnerDocuments.Add(item);
                    }

                    _context.SaveChanges();
                }

                // Tạo share code
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var stringReturn = new string(Enumerable.Repeat(chars, 9).Select(s => s[random.Next(s.Length)]).ToArray());

                // Tạo User cửa hàng admin
                var newUser = new User();
                newUser.id = Guid.NewGuid();
                newUser.code = request.code;
                newUser.full_name = request.name;
                newUser.avatar = request.avatar;
                newUser.email = request.email;
                newUser.phone = request.phone;
                newUser.username = request.username;
                newUser.password = _commonFunction.ComputeSha256Hash(request.password);
                newUser.status = 1;
                newUser.is_sysadmin = false;
                newUser.is_admin = false;
                newUser.is_customer = false;
                newUser.is_partner_admin = true;
                newUser.is_partner = true;
                newUser.partner_id = data.id;
                newUser.total_point = 0;
                newUser.point_waiting = 0;
                newUser.point_avaiable = 0;
                newUser.point_affiliate = 0;
                newUser.share_code = stringReturn;
                newUser.is_delete = false;
                newUser.user_created = username;
                newUser.user_updated = username;
                newUser.date_created = DateTime.Now;
                newUser.date_updated = DateTime.Now;
                newUser.send_Popup = true;
                newUser.send_Notification = true;
                newUser.SMS_addPointSave = true;
                newUser.SMS_addPointUse = true;
                _context.Users.Add(newUser);
                // Save Changes
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

        public APIResponse update(PartnerRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Partners.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            if (request.service_type_id == null)
            {
                return new APIResponse("ERROR_SERVICE_TYPE_ID_MISSING");
            }

            if (request.store_type_id == null)
            {
                return new APIResponse("ERROR_STORE_TYPE_ID_MISSING");
            }

            if (request.phone == null)
            {
                return new APIResponse("ERROR_PHONE_MISSING");
            }

            if (request.start_hour == null)
            {
                return new APIResponse("ERROR_START_HOUR_MISSING");
            }

            if (request.end_hour == null)
            {
                return new APIResponse("ERROR_END_HOUR_MISSING");
            }

            if(request.bk_partner_code != null && request.Encrypt_status == 0)
            {
                return new APIResponse("ERROR_Encrypt");
            }

            try
            {
                data.name = request.name;
                data.service_type_id = request.service_type_id;
                data.store_type_id = request.store_type_id;
                data.store_owner = request.store_owner;
                data.avatar = request.avatar;
                data.phone = request.phone;
                data.email = request.email;
                data.start_hour = request.start_hour;
                data.end_hour = request.end_hour;
                data.working_day = request.working_day;
                data.tax_tncn = request.tax_tncn;
                data.tax_code = request.tax_code;
                data.description = request.description;
                data.product_label_id = request.product_label_id;
                data.province_id = request.province_id;
                data.district_id = request.district_id;
                data.ward_id = request.ward_id;
                data.address = request.address;
                data.latitude = request.latitude;
                data.longtitude = request.longtitude;
                data.support_person_id = request.support_person_id;
                data.support_person_phone = request.support_person_phone;

                // Bổ sung 14/09
                data.license_image = request.license_image;
                data.license_no = request.license_no;
                data.license_person_number = request.license_person_number;
                data.license_owner = request.license_owner;
                if (request.license_date != null)
                {
                    data.license_date = _commonFunction.convertStringSortToDate(request.license_date);
                }

                if (request.license_birth_date != null)
                {
                    data.license_birth_date = _commonFunction.convertStringSortToDate(request.license_birth_date);
                }

                if (request.identifier_date != null)
                {
                    data.identifier_date = _commonFunction.convertStringSortToDate(request.identifier_date);
                }

                if (request.identifier_date_expire != null)
                {
                    data.identifier_date_expire = _commonFunction.convertStringSortToDate(request.identifier_date_expire);
                }
                data.license_nation_id = request.license_nation_id;
                data.indetifier_no = request.indetifier_no;
                data.identifier_at = request.identifier_at;
                data.identifier_address = request.identifier_address;
                data.identifier_province_id = request.identifier_province_id;
                data.is_same_address = request.is_same_address != null ? request.is_same_address : false;
                data.now_address = request.now_address;
                data.now_nation_id = request.now_nation_id;
                data.now_province_id = request.now_province_id;
                data.identifier_front_image = request.identifier_front_image;
                data.identifier_back_image = request.identifier_back_image;
                data.owner_percent = request.owner_percent;
                data.identifier_nation_id = request.identifier_nation_id;
                data.status = request.status;

                data.bk_partner_code = request.bk_partner_code;
                data.bk_merchant_id =  request.bk_merchant_id ;
                data.bk_email = request.bk_email;
                data.bk_password = request.bk_password;
                data.bk_bank_id = request.bk_bank_id;
                data.bk_bank_no = request.bk_bank_no;
                data.bk_bank_owner = request.bk_bank_owner;
                data.bk_bank_name = request.bk_bank_name;
                data.API_SECRET =request.API_SECRET  ;
                data.API_KEY =  request.API_KEY  ;
                data.Encrypt_status = request.Encrypt_status;
                data.RSA_privateKey = RSASign.rplRSAPrivateKey(request.RSA_privateKey);
                data.RSA_publicKey = RSASign.rplRSAPublicKey(request.RSA_publicKey);
                data.link_QR = request.link_QR;
                data.user_updated = username;
                data.date_updated = DateTime.Now;
                _context.SaveChanges();

                var lstDeletes = _context.PartnerDocuments.Where(x => x.partner_id == data.id).ToList();
                _context.PartnerDocuments.RemoveRange(lstDeletes);

                _context.SaveChanges();

                // Tạo tài liệu
                if (request.list_documents != null && request.list_documents.Count > 0)
                {
                    for (int i = 0; i < request.list_documents.Count; i++)
                    {
                        var item = new PartnerDocument();
                        item.id = Guid.NewGuid();
                        item.partner_id = data.id;
                        item.file_name = request.list_documents[i].file_name;
                        item.links = request.list_documents[i].links;
                        _context.PartnerDocuments.Add(item);
                    }

                    _context.SaveChanges();
                }

                var userObj = _context.Users.Where(x => x.partner_id == data.id && x.is_partner_admin == true).FirstOrDefault();

                if (userObj == null && request.username != null && request.username.Length > 0 && request.status == 15)
                {
                    // Tạo share code
                    Random random = new Random();
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var stringReturn = new string(Enumerable.Repeat(chars, 9).Select(s => s[random.Next(s.Length)]).ToArray());

                    // Tạo User cửa hàng admin
                    var newUser = new User();
                    newUser.id = Guid.NewGuid();
                    newUser.code = request.code;
                    newUser.full_name = request.name;
                    newUser.avatar = request.avatar;
                    newUser.email = request.email;
                    newUser.phone = request.phone;
                    newUser.username = request.username;
                    newUser.password = _commonFunction.ComputeSha256Hash(request.password);
                    newUser.status = 1;
                    newUser.is_sysadmin = false;
                    newUser.is_admin = false;
                    newUser.is_customer = false;
                    newUser.is_partner_admin = true;
                    newUser.is_partner = true;
                    newUser.partner_id = data.id;
                    newUser.total_point = 0;
                    newUser.point_waiting = 0;
                    newUser.point_avaiable = 0;
                    newUser.point_affiliate = 0;
                    newUser.share_code = stringReturn;
                    newUser.is_delete = false;
                    newUser.user_created = request.name;
                    newUser.user_updated = request.name;
                    newUser.date_created = DateTime.Now;
                    newUser.date_updated = DateTime.Now;
                    _context.Users.Add(newUser);
                    // Save Changes
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse updateInStore(PartnerRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Partners.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.start_hour == null)
            {
                return new APIResponse("ERROR_START_HOUR_MISSING");
            }

            if (request.end_hour == null)
            {
                return new APIResponse("ERROR_END_HOUR_MISSING");
            }

            if (request.Encrypt_status == 0)
            {
                return new APIResponse("ERROR_Encrypt");
            }

            try
            {
                data.start_hour = request.start_hour;
                data.end_hour = request.end_hour;
                data.avatar = request.avatar;
                data.working_day = request.working_day;
                data.description = request.description;

                data.bk_partner_code = request.bk_partner_code;
                data.bk_merchant_id = request.bk_merchant_id;
                data.bk_email = request.bk_email;
                data.bk_password = request.bk_password;
                data.bk_bank_id = request.bk_bank_id;
                data.bk_bank_no = request.bk_bank_no;
                data.bk_bank_owner = request.bk_bank_owner;
                data.bk_bank_name = request.bk_bank_name;
                data.API_KEY = request.API_KEY;
                data.API_SECRET = request.API_SECRET;
                data.RSA_privateKey = RSASign.rplRSAPublicKey(request.RSA_privateKey);
                data.RSA_publicKey = RSASign.rplRSAPublicKey(request.RSA_publicKey);
                data.link_QR = request.link_QR;
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

        public APIResponse lockAccount(DeleteGuidRequest request)
        {
            var data = _context.Partners.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.status != 15)
            {
                return new APIResponse("ERROR_ACCOUNT_NOT_LOCK");
            }
            var transaction = _context.Database.BeginTransaction();

            var userPartnerAdmin = _context.Users.Where(x => x.partner_id == request.id && x.is_partner_admin == true).FirstOrDefault();

            try
            {
                data.status = 16;
                if (userPartnerAdmin != null)
                {
                    userPartnerAdmin.status = 2;
                }
                _context.SaveChanges();

                // Gửi email
                if (data.email != null)
                {
                    string subjectEmail = "[CashPlus] - Thông báo khóa tài khoản";
                    string mail_to = data.email;
                    string message = "<p>Xin chào " + data.name + ",<p>";
                    message += "<p>Tài khoản của hàng của bạn đã bị khóa có thể do vi phạm <b>Điều khoản & chính sách sử dụng:</b></p>";
                    message += "<p><a href='https://cashplus.vn/chinh-sach-su-dung'>https://cashplus.vn/chinh-sach-su-dung</a></p>";
                    message += "<p>Nếu bạn cần giải đáp thắc mắc hoặc yêu cầu hỗ trợ, vui lòng liên hệ với bộ phận chăm sóc khách hàng của chúng tôi. Chúng tôi sẽ hỗ trợ bạn trong thời gian sớm nhất.</p>";
                    message += "<p>Trân trọng!</p>";
                    message += "<br/>";
                    message += "<p>@2023 ATS Group</p>";

                    _emailSender.SendEmailAsync(mail_to, subjectEmail, message);
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

        public APIResponse unlockAccount(DeleteGuidRequest request)
        {
            var data = _context.Partners.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (data.status != 16)
            {
                return new APIResponse("ERROR_ACCOUNT_NOT_UNLOCK");
            }

            var transaction = _context.Database.BeginTransaction();

            var userPartnerAdmin = _context.Users.Where(x => x.partner_id == request.id && x.is_partner_admin == true).FirstOrDefault();
            try
            {
                data.status = 15;
                if (userPartnerAdmin != null)
                {
                    userPartnerAdmin.status = 1;
                }
                _context.SaveChanges();

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
            var data = _context.Partners.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            var contractObj = _context.PartnerContracts.Where(x => x.partner_id == request.id && x.is_delete != true).FirstOrDefault();
            if (contractObj != null)
            {
                return new APIResponse("ERROR_ACCOUNT_NOT_DELETE");
            }

            var lstUsers = _context.Users.Where(x => x.is_partner == true && x.partner_id == data.id).ToList();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                data.is_delete = true;

                _context.SaveChanges();
                for (int i = 0; i < lstUsers.Count; i++)
                {
                    lstUsers[i].is_delete = true;
                }

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

        public APIResponse changePassword(PasswordRequest request)
        {
            var data = _context.Users.Where(x => x.partner_id == request.user_id && x.is_partner_admin == true).FirstOrDefault();
            var partnerObj = _context.Partners.Where(x => x.id == data.partner_id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                data.password = _commonFunction.ComputeSha256Hash(request.new_password);

                // Gửi email
                if (partnerObj.email != null)
                {
                    string subjectEmail = "[CashPlus] - Đổi mật khẩu thành công";
                    string mail_to = partnerObj.email;
                    string message = "<p>Xin chào " + partnerObj.name + ",<p>";
                    message += "<p>Chúng tôi xin thông báo rằng mật khẩu tài khoản của bạn đã được thay đổi thành công. Nếu bạn không thực hiện việc thay đổi mật khẩu này, vui lòng liên hệ với bộ phận chăm sóc khách hàng của chúng tôi để được hỗ trợ.</p>";
                    message += "<p><a href='https://cashplus.vn/chinh-sach-su-dung'>https://cashplus.vn/chinh-sach-su-dung</a></p>";
                    message += "<p>- Thông tin tài khoản: " + data.username + "/" + request.new_password + "</p>";
                    message += "<p>- Link truy cập: <a href='https://store.cashplus.vn'>https://store.cashplus.vn</a></p>";
                    message += "<p>Trân trọng!</p>";
                    message += "<br/>";
                    message += "<p>@2023 ATS Group</p>";

                    _emailSender.SendEmailAsync(mail_to, subjectEmail, message);
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse getListAccumulatePointOrder(AccumulatePointOrderRequest request)
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
            var lstData = (from p in _context.AccumulatePointOrders
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               customer_phone = c.phone,
                               customer_name = c.full_name,
                               bill_amount = p.bill_amount,
                               point_partner = p.point_partner,
                               point_customer = p.point_customer,
                               approve_user = p.user_created,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Contains(request.trans_no));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
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

        public APIResponse getListChangePointOrder(ChangePointOrderRequest request)
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
            var lstData = (from p in _context.ChangePointOrders
                           join u in _context.Users on p.user_id equals u.id into us
                           from u in us.DefaultIfEmpty()
                           join cb in _context.CustomerBankAccounts on p.customer_bank_account_id equals cb.id into cbs
                           from cb in cbs.DefaultIfEmpty()
                           join b in _context.Banks on cb.bank_id equals b.id into bs
                           from b in bs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.user_id == request.user_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               value_exchange = p.value_exchange,
                               point_exchange = p.point_exchange,
                               bank_name = b != null ? b.name : "",
                               bank_no = cb.bank_no,
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Contains(request.trans_no));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
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

        public APIResponse getListPartnerOrder(PartnerOrderRequest request)
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
            var lstData = (from p in _context.PartnerOrders
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               order_code = p.order_code,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               total_amount = p.total_amount,
                               status = p.status,
                               customer_phone = c.phone,
                               status_name = st != null ? st.name : "",
                               list_items = (from it in _context.PartnerOrderDetails
                                             join pr in _context.Products on it.product_id equals pr.id
                                             where it.partner_order_id == p.id
                                             select new
                                             {
                                                 product_name = pr.name,
                                                 quantity = it.quantity
                                             }).ToList()
                           });

            // Nếu tồn tại Where theo tên
            if (request.order_code != null && request.order_code.Length > 0)
            {
                lstData = lstData.Where(x => x.order_code.Contains(request.order_code));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.trans_date_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
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

        public APIResponse getListAccumulatePointOrderRating(AccumulatePointOrderRatingRequest request)
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
            var lstData = (from p in _context.AccumulatePointOrderRatings
                           join ord in _context.AccumulatePointOrders on p.accumulate_point_order_id equals ord.id
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = ord.trans_no,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringSort(p.date_created),
                               customer_phone = c.phone,
                               customer_name = c.full_name,
                               content = p.content,
                               rating = p.rating,
                               rating_name = p.rating_name
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Contains(request.trans_no));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
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

        public APIResponse getListAddPointOrder(AddPointOrderRequest request)
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
            var lstData = (from p in _context.AddPointOrders
                           join fb in _context.CustomerFakeBanks on p.id equals fb.user_id into fbs
                           from fb in fbs.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               date_created_origin = p.date_created,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_exchange,
                               date_created = _commonFunction.convertDateToStringSort(p.date_created),
                               bank_name = fb != null ? fb.bank_name : "",
                               bank_no = fb != null ? fb.bank_account : "",
                               status_name = "Hoàn thành"
                           });

            // Nếu tồn tại Where theo tên
            if (request.trans_no != null && request.trans_no.Length > 0)
            {
                lstData = lstData.Where(x => x.trans_no.Contains(request.trans_no));
            }

            if (request.from_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin >= _commonFunction.convertStringSortToDate(request.from_date).Date);
            }

            if (request.to_date != null)
            {
                lstData = lstData.Where(x => x.date_created_origin <= _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1));
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

        public APIResponse getListTeam(PartnerRequest request)
        {
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

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            var userObj = _context.Users.Where(x => x.partner_id == request.partner_id && x.is_partner_admin == true).FirstOrDefault();

            if (userObj == null)
            {
                return new APIResponse("ERROR_USER_ID_INCORRECT");
            }

            var from_date = request.from_date != null ? _commonFunction.convertStringSortToDate(request.from_date).Date : _commonFunction.convertStringSortToDate("01/01/2020");
            var to_date = request.to_date != null ? _commonFunction.convertStringSortToDate(request.to_date).Date.AddDays(1).AddTicks(-1) : _commonFunction.convertStringSortToDate("01/01/2900");

            var lstTeamLevel1 = (from p in _context.Users
                                 join sh in _context.Users on p.share_person_id equals sh.id
                                 join c in _context.Customers on p.customer_id equals c.id into cs
                                 from c in cs.DefaultIfEmpty()
                                 join st in _context.OtherLists on p.status equals st.id
                                 where p.share_person_id == userObj.id
                                 select new
                                 {
                                     avatar = p.avatar,
                                     full_name = c.full_name,
                                     share_code = p.share_code,
                                     phone = c.phone,
                                     share_person_name = sh.full_name,
                                     status = p.status,
                                     status_name = st.name,
                                     date_created = c.date_created,
                                     level = 1,
                                     total_point = p.total_point,
                                     count_person = _context.Users.Where(x => x.share_person_id == p.id).Count(),
                                     total_point_accumulate = (from tp in _context.PartnerPointHistorys
                                                               where tp.order_type == "AFF_LV_1" && tp.source_id == p.customer_id && tp.trans_date >= from_date && tp.trans_date <= to_date
                                                               select new
                                                               {
                                                                   point_amount = tp.point_amount
                                                               }).Sum(x => x.point_amount)
                                 });

            //var idsLevel1 = _context.Users.Where(x => x.share_person_id == userObj.id).Select(x => x.id).ToList();
            //var lstTeamLevel2 = (from p in _context.Users
            //                     join sh in _context.Users on p.share_person_id equals sh.id
            //                     join c in _context.Customers on p.customer_id equals c.id into cs
            //                     from c in cs.DefaultIfEmpty()
            //                     join st in _context.OtherLists on p.status equals st.id
            //                     where idsLevel1.Contains(p.share_person_id)
            //                     select new
            //                     {
            //                         avatar = p.avatar,
            //                         full_name = c.full_name,
            //                         share_code = p.share_code,
            //                         phone = c.phone,
            //                         share_person_name = sh.full_name,
            //                         status = p.status,
            //                         status_name = st.name,
            //                         date_created = c.date_created,
            //                         level = 2,
            //                         total_point = p.total_point,
            //                         count_person = _context.Users.Where(x => x.share_person_id == p.id).Count(),
            //                         total_point_accumulate = (from tp in _context.PartnerPointHistorys
            //                                                   where tp.order_type == "AFF_LV_2" && tp.source_id == p.customer_id && tp.trans_date >= from_date && tp.trans_date <= to_date
            //                                                   select new
            //                                                   {
            //                                                       point_amount = tp.point_amount
            //                                                   }).Sum(x => x.point_amount)
            //                     });

            //var idsLevel2 = _context.Users.Where(x => idsLevel1.Contains(x.share_person_id)).Select(x => x.id).ToList();
            //var lstTeamLevel3 = (from p in _context.Users
            //                     join sh in _context.Users on p.share_person_id equals sh.id
            //                     join c in _context.Customers on p.customer_id equals c.id into cs
            //                     from c in cs.DefaultIfEmpty()
            //                     join st in _context.OtherLists on p.status equals st.id
            //                     where idsLevel2.Contains(p.share_person_id)
            //                     select new
            //                     {
            //                         avatar = p.avatar,
            //                         full_name = c.full_name,
            //                         share_code = p.share_code,
            //                         phone = c.phone,
            //                         share_person_name = sh.full_name,
            //                         status = p.status,
            //                         status_name = st.name,
            //                         date_created = c.date_created,
            //                         level = 3,
            //                         total_point = p.total_point,
            //                         count_person = _context.Users.Where(x => x.share_person_id == p.id).Count(),
            //                         total_point_accumulate = (from tp in _context.PartnerPointHistorys
            //                                                   where tp.order_type == "AFF_LV_3" && tp.source_id == p.customer_id && tp.trans_date >= from_date && tp.trans_date <= to_date
            //                                                   select new
            //                                                   {
            //                                                       point_amount = tp.point_amount
            //                                                   }).Sum(x => x.point_amount)
            //                     });


            //var lstResult = lstTeamLevel1.Concat(lstTeamLevel2).Concat(lstTeamLevel3);
            var lstResult = lstTeamLevel1;

            // Tìm kiếm
            if (request.search != null && request.search.Length > 0)
            {
                lstResult = lstResult.Where(x => x.full_name.Contains(request.search) || x.phone.Contains(request.search) || x.share_code.Contains(request.search));
            }

            //if (request.level != null)
            //{
            //    lstResult = lstResult.Where(x => x.level == request.level);
            //}


            if (request.order_by_condition != null)
            {
                if (request.order_by_type == "ASC")
                {
                    if (request.order_by_condition == "DATE_CREATED")
                    {
                        lstResult = lstResult.OrderBy(x => x.date_created);
                    }
                    else if (request.order_by_condition == "TOTAL_POINT")
                    {
                        lstResult = lstResult.OrderBy(x => x.total_point_accumulate);
                    }
                }
                else
                {
                    if (request.order_by_condition == "DATE_CREATED")
                    {
                        lstResult = lstResult.OrderByDescending(x => x.date_created);
                    }
                    else if (request.order_by_condition == "TOTAL_POINT")
                    {
                        lstResult = lstResult.OrderByDescending(x => x.total_point_accumulate);
                    }
                }
            }
            else
            {
                lstResult = lstResult.OrderByDescending(x => x.date_created);
            }

            // Đếm số lượng
            int countElements = lstResult.Count();
            //var values = lstResult.Sum(x => x.total_point_accumulate);

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstResult.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList, values = 0, data_count = countElements };

            return new APIResponse(dataResult);
        }
    }
}
