using LOYALTY.Data;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using LOYALTY.Interfaces;
using LOYALTY.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataAccess
{

    public class AppCustomerHomeDataAccess : IAppCustomerHome
    {
        public class PartnerHomeResponse
        {
            public Guid? id { get; set; }
            public string? name { get; set; }
            public string? code { get; set; }
            public string? phone { get; set; }
            public string? address { get; set; }
            public string? start_hour { get; set; }
            public string? end_hour { get; set; }
            public string? working_day { get; set; }
            public string? avatar { get; set; }
            public double? latitude { get; set; }
            public double? longtitude { get; set; }
            public int? province_id { get; set; }
            public string? province_name { get; set; }
            public int? district_id { get; set; }
            public int? ward_id { get; set; }
            public Guid? service_type_id { get; set; }
            public Guid? product_label_id { get; set; }
            public string? service_type_icons { get; set; }
            public Boolean? is_favourite { get; set; }
            public decimal? contract_discount_rate { get; set; }
            public decimal? discount_rate { get; set; }
            public decimal? rating { get; set; }
            public decimal? total_rating { get; set; }
            public Boolean? is_hide_discount { get; set; }
            public double? distance { get; set; }
            public Guid? contract_id { get; set; }
            public bool is_priority { get; set; } = false;
        }
        public class reqPoint
        {
            public DateTime? from_date { get; set; }
            public DateTime? to_date { get; set; }

            public Guid customer_id { get; set; }

        }

        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public AppCustomerHomeDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getHomeInfo(Guid customer_id)
        {
            var dateNow = DateTime.Now;
            var settings = _context.Settingses.FirstOrDefault();

            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;
            var dataResponse = (from p in _context.Customers
                                join u in _context.Users on p.id equals u.customer_id
                                join st in _context.OtherLists on p.status equals st.id into sts
                                from st in sts.DefaultIfEmpty()
                                join cr in _context.CustomerRanks on p.customer_rank_id equals cr.id into crs
                                from cr in crs.DefaultIfEmpty()
                                join crd in _context.CustomerRanks on Guid.Parse("26C4F206-6B1C-4702-851C-8BAC40CFC2BB") equals crd.id into crds
                                from crd in crds.DefaultIfEmpty()
                                where p.id == customer_id
                                select new
                                {
                                    customer_id = p.id,
                                    customer_name = p.full_name,
                                    customer_phone = p.phone,
                                    customer_avatar = p.avatar,
                                    customer_email = p.email,
                                    customer_address = p.address,
                                    customer_birth_date = p.birth_date != null ? _commonFunction.convertDateToStringSort(p.birth_date) : "",
                                    customer_status = p.status,
                                    customer_status_name = st != null ? st.name : "",
                                    rank_code = cr != null ? cr.code : (crd != null ? crd.code : ""),
                                    rank_name = cr != null ? cr.name : (crd != null ? crd.name : ""),
                                    rank_content = cr != null ? cr.content : (crd != null ? crd.content : ""),
                                    share_code = u.share_code,
                                    share_code_link = Consts.LINK_SHARE + u.share_code,
                                    total_point = u.total_point,
                                    point_waiting = u.point_waiting,
                                    point_affiliate = u.point_affiliate,
                                    point_avaiable = u.point_avaiable,
                                    point_exchange_rate = pointExchangeRate,
                                    min_change_point = settings.change_point_estimate,
                                    max_change_point = settings.approve_change_point_min,
                                    point_value = settings.point_value,
                                    point_exchange = settings.point_exchange,
                                    is_have_secret_key = u.secret_key != null ? true : false,
                                    consumption_point_description = settings.consumption_point_description,
                                    affiliate_point_description = settings.affiliate_point_description,
                                    app_customer_banner_image = settings.app_customer_banner_image,
                                    app_customer_spinner_image = settings.app_customer_spinner_image,
                                    app_customer_wallet_image = settings.app_customer_wallet_image,
                                    app_customer_qr_image = settings.app_customer_qr_image,
                                    app_customer_affiliate_image = settings.app_customer_affiliate_image,
                                    app_customer_gift_image = settings.app_customer_gift_image,
                                    list_banners = _context.Banners.Where(x => x.start_date <= dateNow && x.end_date >= dateNow).Take(5).Skip(0).ToList(),
                                    list_product_labels = _context.ProductLabels.Where(x => x.status == 1).OrderBy(x => x.orders).Take(6).Skip(0).ToList(),
                                    list_rating_configs = _context.RatingConfigs.OrderBy(x => x.rating).Take(5).Skip(0).ToList(),
                                    is_have_bank = _context.CustomerBankAccounts.Where(x => x.user_id == customer_id).FirstOrDefault() != null ? true : false,
                                    amount_limit = settings.amount_limit,
                                    send_Notification = u.send_Notification,
                                    send_Popup = u.send_Popup,
                                    SMS_addPointSave = u.SMS_addPointSave,
                                    SMS_addPointUse = u.SMS_addPointUse,
                                }).FirstOrDefault();

            return new APIResponse(dataResponse);
        }

        public APIResponse getHomeGeneralInfo()
        {
            var dateNow = DateTime.Now;
            var settings = _context.Settingses.FirstOrDefault();

            var listBanners = _context.Banners.Where(x => x.start_date <= dateNow && x.end_date >= dateNow).ToList();
            var listProductLabels = _context.ProductLabels.Where(x => x.status == 1).OrderBy(x => x.orders).Take(6).Skip(0).ToList();
            var dataResponse = new
            {
                listBanners = listBanners,
                listProductLabels = listProductLabels,
                app_customer_banner_image = settings.app_customer_banner_image,
                app_customer_spinner_image = settings.app_customer_spinner_image,
                app_customer_wallet_image = settings.app_customer_wallet_image,
                app_customer_qr_image = settings.app_customer_qr_image,
                app_customer_affiliate_image = settings.app_customer_affiliate_image,
                app_customer_gift_image = settings.app_customer_gift_image
            };

            return new APIResponse(dataResponse);
        }

        public APIResponse getPointByTime(reqPoint request)
        {
            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }


            if (request.from_date == null)
            {
                request.from_date = _context.Customers.Where(l => l.id == request.customer_id).Select(p => p.date_created).FirstOrDefault();
            }

            if (request.to_date == null)
            {
                request.to_date = DateTime.Now;
            }

            long point_accumulation = 0;

            var total_point = _context.AccumulatePointOrders.Where(x => x.customer_id == request.customer_id && (x.approve_date != null && x.approve_date >= request.from_date && x.approve_date <= request.to_date) && x.return_type == "Cash").Select(x => x.point_customer).Sum(x => x.Value);

            if (total_point != null)
            {
                point_accumulation = long.Parse(total_point.ToString());
            }

            return new APIResponse(new
            {
                point_accumulation = point_accumulation
            });
        }

        public APIResponse getSuggestSearch(PartnerRequest request)
        {
            var data = _context.AppSuggestSearchs.Where(x => (request.service_type_id != null ? (x.service_type_id == request.service_type_id) : (1 == 1))).Select(x => x.name).ToList();

            return new APIResponse(new
            {
                data = data
            });
        }

        public APIResponse getTotalNotificationNotRead(Guid customer_id)
        {

            var totalNotification = (from p in _context.Notifications
                                     where p.user_id == null || (p.user_id != null && p.user_id == customer_id)
                                     select p.id).Count();


            var totalNotificationRead = (from p in _context.Notifications
                                         join cp in _context.UserNotifications on p.id equals cp.notification_id
                                         where (p.user_id == null || (p.user_id != null && p.user_id == customer_id)) && cp.user_id == customer_id
                                         select p.id).Count();

            var total_noti_not_read = totalNotification - totalNotificationRead;

            return new APIResponse(new
            {
                total_noti_not_read = total_noti_not_read
            });

        }
        public async Task<APIResponse> getListPartner(PartnerRequest request, string username)
        {
            try
            {
                var customerId = _context.Users.Where(x => x.username == username && x.is_customer == true).Select(x => x.customer_id).FirstOrDefault();

                request.page_size = 50;

                if (request.page_no < 1)
                {
                    request.page_no = 1;
                }

                decimal generalCustomerRate = 0;

                var generalId = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").Select(x => x.id).FirstOrDefault();

                var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == generalId).Select(x => new
                {
                    allocation_name = x.allocation_name,
                    discount_rate = x.discount_rate
                }).ToList();

                for (int j = 0; j < listAccuDetails.Count; j++)
                {
                    if (listAccuDetails[j].allocation_name == "Khách hàng")
                    {
                        generalCustomerRate = (decimal)listAccuDetails[j].discount_rate;
                    }
                }

                // Số lượng Skip
                int skipElements = (request.page_no - 1) * request.page_size;

                var lstData = (from p in _context.Partners
                               join f in _context.PartnerFavourites.Where(p => p.customer_id == customerId) on p.id equals f.partner_id into fs
                               from f in fs.DefaultIfEmpty()
                               join con in _context.PartnerContracts.Where(x => x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12) on p.id equals con.partner_id into cons
                               from con in cons.DefaultIfEmpty()
                               join pr in _context.Provinces on p.province_id equals pr.id into prs
                               from pr in prs.DefaultIfEmpty()
                               join di in _context.Provinces on p.district_id equals di.id into dis
                               from di in dis.DefaultIfEmpty()
                               join wa in _context.Provinces on p.ward_id equals wa.id into was
                               from wa in was.DefaultIfEmpty()
                               join st in _context.ServiceTypes on p.service_type_id equals st.id into sts
                               from st in sts.DefaultIfEmpty()
                               where p.status == 15
                               orderby p.rating descending
                               select new PartnerHomeResponse
                               {
                                   id = p.id,
                                   name = p.name,
                                   code = p.code,
                                   phone = p.phone,
                                   address = p.address,
                                   start_hour = p.start_hour,
                                   end_hour = p.end_hour,
                                   working_day = p.working_day,
                                   avatar = p.avatar,
                                   latitude = (p.latitude != null && p.latitude.Length > 0) ? float.Parse(p.latitude) : float.Parse("0"),
                                   longtitude = (p.longtitude != null && p.longtitude.Length > 0) ? float.Parse(p.longtitude) : float.Parse("0"),
                                   province_id = p.province_id,
                                   province_name = pr.name,
                                   district_id = p.district_id,
                                   ward_id = p.ward_id,
                                   service_type_id = p.service_type_id,
                                   product_label_id = p.product_label_id,
                                   service_type_icons = st != null ? st.icons : null,
                                   is_favourite = f != null ? true : false,
                                   discount_rate = p.customer_discount_rate,
                                   rating = p.rating,
                                   contract_discount_rate = con != null ? con.discount_rate : 0,
                                   total_rating = p.total_rating,
                                   is_hide_discount = false,
                                   contract_id = con != null ? con.id : null
                               });

                if (request.product_group_id != null)
                {
                    var lstPartner = await (from p in _context.Products
                                      where p.product_group_id == request.product_group_id
                                      group p by new
                                      {
                                          p.partner_id,
                                      } into e
                                      select new
                                      {
                                          e.Key.partner_id,
                                      }).ToListAsync();
                    List<Guid> lstPartners = new List<Guid>();
                    foreach (var item in lstPartner)
                    {
                        lstPartners.Add(item.partner_id.Value);
                    }

                    lstData = (from p in _context.Partners
                               join f in _context.PartnerFavourites.Where(x => x.customer_id == customerId) on p.id equals f.partner_id into fs
                               from f in fs.DefaultIfEmpty()
                               join con in _context.PartnerContracts.Where(x => x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12) on p.id equals con.partner_id into cons
                               from con in cons.DefaultIfEmpty()
                               join pr in _context.Provinces on p.province_id equals pr.id into prs
                               from pr in prs.DefaultIfEmpty()
                               join di in _context.Provinces on p.district_id equals di.id into dis
                               from di in dis.DefaultIfEmpty()
                               join wa in _context.Provinces on p.ward_id equals wa.id into was
                               from wa in was.DefaultIfEmpty()
                               join st in _context.ServiceTypes on p.service_type_id equals st.id into sts
                               from st in sts.DefaultIfEmpty()
                               where p.status == 15
                               && (lstPartners.Contains(p.id.Value))
                               orderby p.rating descending
                               select new PartnerHomeResponse
                               {
                                   id = p.id,
                                   name = p.name,
                                   code = p.code,
                                   phone = p.phone,
                                   address = p.address,
                                   start_hour = p.start_hour,
                                   end_hour = p.end_hour,
                                   working_day = p.working_day,
                                   avatar = p.avatar,
                                   latitude = (p.latitude != null && p.latitude.Length > 0) ? float.Parse(p.latitude) : float.Parse("0"),
                                   longtitude = (p.longtitude != null && p.longtitude.Length > 0) ? float.Parse(p.longtitude) : float.Parse("0"),
                                   province_id = p.province_id,
                                   province_name = pr.name,
                                   district_id = p.district_id,
                                   ward_id = p.ward_id,
                                   service_type_id = p.service_type_id,
                                   product_label_id = p.product_label_id,
                                   service_type_icons = st != null ? st.icons : null,
                                   is_favourite = f != null ? true : false,
                                   contract_discount_rate = con.discount_rate != null ? con.discount_rate : 0,
                                   discount_rate = p.customer_discount_rate,
                                   rating = p.rating,
                                   total_rating = p.total_rating,
                                   is_hide_discount = false,
                                   contract_id = con != null ? con.id : null
                               });
                }

                // Lọc cửa hàng có hợp đồng hiệu lực
                lstData = lstData.Where(x => x.contract_id != null);

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

                if (request.service_type_id != null)
                {
                    lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
                }

                if (request.product_label_id != null)
                {
                    lstData = lstData.Where(x => x.product_label_id == request.product_label_id);
                }

                List<Guid> list_partner_ids = new List<Guid>();

                if (request.search != null && request.search.Length > 0)
                {
                    var lstProducts = (from p in _context.Products
                                       join pg in _context.ProductGroups on p.product_group_id equals pg.id
                                       where p.name.ToLower().Contains(request.search.ToLower()) || pg.name.ToLower().Contains(request.search.ToLower())
                                       select (Guid)p.partner_id).Distinct ();

                    list_partner_ids = await lstProducts.ToListAsync();

                    lstData = lstData.Where(x => x.name.ToLower().Contains(request.search.ToLower()) || list_partner_ids.Contains((Guid)x.id));
                }

                // Đếm số lượng
                int countElements = await lstData.CountAsync();

                // Số lượng trang
                int totalPage = countElements > 0
                        ? (int)Math.Ceiling(countElements / (double)request.page_size)
                        : 0;

                double zoom = 0.01;

                // Data Sau phân trang
                //var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();

                var dataList = (await lstData.ToListAsync()).GroupBy(p => p.id).Select(p => p.First()).ToList();

                var dateNow = DateTime.Now;

                for (int i = 0; i < dataList.Count; i++)
                {
                    decimal customerExchange = 0;

                    // Lấy cấu hình đổi điểm hiệu lực
                    var accumulateConfigId = await _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == dataList[i].id && x.status == 23).Select(x => x.id).FirstOrDefaultAsync();

                    // Nếu không có riêng thì lấy chung
                    if (accumulateConfigId != null)
                    {
                        var listAccuDetail2s = await _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfigId).Select(x => new
                        {
                            allocation_name = x.allocation_name,
                            discount_rate = x.discount_rate
                        }).ToListAsync();


                        for (int j = 0; j < listAccuDetail2s.Count; j++)
                        {
                            if (listAccuDetail2s[j].allocation_name == "Khách hàng")
                            {
                                customerExchange = (decimal)listAccuDetail2s[j].discount_rate;
                            }
                        }
                        dataList[i].discount_rate = Math.Round((decimal)dataList[i].contract_discount_rate * customerExchange / 10) / 10;
                    }
                    else
                    {
                        if(dataList[i].contract_discount_rate == null)
                        {
                            dataList[i].contract_discount_rate = 0;
                        }
                        dataList[i].discount_rate = Math.Round((decimal)dataList[i].contract_discount_rate * generalCustomerRate / 10) / 10;
                    }

                    dataList[i].distance = (request.latitude != null && request.longtitude != null && dataList[i].latitude != null && dataList[i].longtitude != null) ? CalculateDistance(request.latitude, request.longtitude, dataList[i].latitude.ToString(), dataList[i].longtitude.ToString()) : 0;
                    if (dataList[i].distance < 3000 || dataList[i].is_favourite == true)
                    {
                        dataList[i].is_hide_discount = false;
                    }
                    else
                    {
                        dataList[i].is_hide_discount = true;
                    }
                }

                var maxDistance = dataList.Max(x => x.distance);
                if (maxDistance < 1000)
                {
                    zoom = 0.01;
                }
                else if (maxDistance >= 1000 && maxDistance < 5000)
                {
                    zoom = 0.03;
                }
                else if (maxDistance >= 5000 && maxDistance < 20000)
                {
                    zoom = 0.05;
                }
                else if (maxDistance >= 20000)
                {
                    zoom = 0.1;
                }


                var dataResult = new { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, zoom = zoom, maxDistance = maxDistance, data = dataList };
                return new APIResponse(dataResult);
            }
            catch (Exception ex)
            {
                return new APIResponse(ex);
            }

        }

        public APIResponse getListPartnerV1(PartnerRequest request, string username)
        {
            var customerId = _context.Users.Where(x => x.username == username && x.is_customer == true).Select(x => x.customer_id).FirstOrDefault();

            request.page_size = 500;

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;

            var lstData = (from p in _context.Partners
                           join f in _context.PartnerFavourites.Where(x => x.customer_id == customerId) on p.id equals f.partner_id into fs
                           from f in fs.DefaultIfEmpty()
                           join pr in _context.Provinces on p.province_id equals pr.id into prs
                           from pr in prs.DefaultIfEmpty()
                           join di in _context.Provinces on p.district_id equals di.id into dis
                           from di in dis.DefaultIfEmpty()
                           join wa in _context.Provinces on p.ward_id equals wa.id into was
                           from wa in was.DefaultIfEmpty()
                           join st in _context.ServiceTypes on p.service_type_id equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.status == 15
                           orderby p.rating descending
                           select new PartnerHomeResponse
                           {
                               id = p.id,
                               name = p.name,
                               code = p.code,
                               phone = p.phone,
                               address = p.address,
                               start_hour = p.start_hour,
                               end_hour = p.end_hour,
                               working_day = p.working_day,
                               avatar = p.avatar,
                               latitude = (p.latitude != null && p.latitude.Length > 0) ? float.Parse(p.latitude) : float.Parse("0"),
                               longtitude = (p.longtitude != null && p.longtitude.Length > 0) ? float.Parse(p.longtitude) : float.Parse("0"),
                               province_id = p.province_id,
                               province_name = pr.name,
                               district_id = p.district_id,
                               ward_id = p.ward_id,
                               service_type_id = p.service_type_id,
                               product_label_id = p.product_label_id,
                               service_type_icons = st != null ? st.icons : null,
                               is_favourite = f != null ? true : false,
                               discount_rate = p.customer_discount_rate,
                               rating = p.rating,
                               total_rating = p.total_rating,
                               is_hide_discount = false,
                               contract_id = _context.PartnerContracts.Where(x => x.partner_id == p.id && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12).Select(x => x.id).FirstOrDefault()
                           });

            // Lọc cửa hàng có hợp đồng hiệu lực
            lstData = lstData.Where(x => x.contract_id != null);

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

            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
            }

            if (request.product_label_id != null)
            {
                lstData = lstData.Where(x => x.product_label_id == request.product_label_id);
            }

            List<Guid> list_partner_ids = new List<Guid>();

            if (request.search != null && request.search.Length > 0)
            {
                var lstProducts = (from p in _context.Products
                                   join pg in _context.ProductGroups on p.product_group_id equals pg.id
                                   where p.name.ToLower().Contains(request.search.ToLower()) || pg.name.ToLower().Contains(request.search.ToLower())
                                   select (Guid)p.partner_id).Distinct();

                list_partner_ids = lstProducts.ToList();

                lstData = lstData.Where(x => x.name.ToLower().Contains(request.search.ToLower()) || list_partner_ids.Contains((Guid)x.id));
            }

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            double zoom = 0.01;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();

            for (int i = 0; i < dataList.Count; i++)
            {
                dataList[i].distance = (request.latitude != null && request.longtitude != null && dataList[i].latitude != null && dataList[i].longtitude != null) ? CalculateDistance(request.latitude, request.longtitude, dataList[i].latitude.ToString(), dataList[i].longtitude.ToString()) : 0;
                if (dataList[i].distance < 3000 || dataList[i].is_favourite == true)
                {
                    dataList[i].is_hide_discount = false;
                }
                else
                {
                    dataList[i].is_hide_discount = true;
                }
            }

            var maxDistance = dataList.Max(x => x.distance);
            if (maxDistance < 1000)
            {
                zoom = 0.01;
            }
            else if (maxDistance >= 1000 && maxDistance < 5000)
            {
                zoom = 0.03;
            }
            else if (maxDistance >= 5000 && maxDistance < 20000)
            {
                zoom = 0.05;
            }
            else if (maxDistance >= 20000)
            {
                zoom = 0.1;
            }
            var dataResult = new { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, zoom = zoom, maxDistance = maxDistance, data = dataList };
            return new APIResponse(dataResult);
        }

        public double CalculateDistance(string lat1, string long1, string lat2, string long2)
        {
            var d1 = double.Parse(lat1) * (Math.PI / 180.0);
            var num1 = double.Parse(long1) * (Math.PI / 180.0);
            var d2 = double.Parse(lat2) * (Math.PI / 180.0);
            var num2 = double.Parse(long2) * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public double CalculateDistanceV2(double lat1, double long1, double lat2, double long2)
        {
            var d1 = lat1 * (Math.PI / 180.0);
            var num1 = long1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var num2 = long2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public APIResponse likePartner(PartnerFavourite request, string username)
        {
            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            var userObj = _context.Users.Where(x => x.username == username && x.is_customer == true).FirstOrDefault();

            if (userObj == null)
            {
                return new APIResponse("ERROR_USER_NOT_FOUND");
            }

            try
            {
                var dataFavourite = _context.PartnerFavourites.Where(x => x.partner_id == request.partner_id && x.customer_id == userObj.customer_id).FirstOrDefault();

                if (dataFavourite == null)
                {
                    var newData = new PartnerFavourite();
                    newData.customer_id = userObj.customer_id;
                    newData.partner_id = request.partner_id;

                    _context.PartnerFavourites.Add(newData);

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }
            return new APIResponse(200);
        }

        public APIResponse dislikePartner(PartnerFavourite request, string username)
        {
            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            var userObj = _context.Users.Where(x => x.username == username && x.is_customer == true).FirstOrDefault();

            if (userObj == null)
            {
                return new APIResponse("ERROR_USER_NOT_FOUND");
            }

            try
            {
                var dataFavourite = _context.PartnerFavourites.Where(x => x.partner_id == request.partner_id && x.customer_id == userObj.customer_id).FirstOrDefault();

                if (dataFavourite != null)
                {
                    _context.PartnerFavourites.Remove(dataFavourite);

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return new APIResponse(400);
            }
            return new APIResponse(200);
        }

        public APIResponse getListPartnerFavourite(PartnerRequest request, string username)
        {

            var userObj = _context.Users.Where(x => x.username == username && x.is_customer == true).FirstOrDefault();

            if (userObj == null)
            {
                return new APIResponse("ERROR_USER_NOT_FOUND");
            }

            request.page_size = 10000;

            if (request.page_no < 1)
            {
                request.page_no = 1;
            }
            // Số lượng Skip
            int skipElements = (request.page_no - 1) * request.page_size;

            var lstData = (from p in _context.Partners
                           join f in _context.PartnerFavourites on p.id equals f.partner_id
                           join pr in _context.Provinces on p.province_id equals pr.id into prs
                           from pr in prs.DefaultIfEmpty()
                           join di in _context.Provinces on p.district_id equals di.id into dis
                           from di in dis.DefaultIfEmpty()
                           join wa in _context.Provinces on p.ward_id equals wa.id into was
                           from wa in was.DefaultIfEmpty()
                           join st in _context.ServiceTypes on p.service_type_id equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.status == 15 && f.customer_id == userObj.customer_id
                           select new
                           {
                               id = p.id,
                               name = p.name,
                               code = p.code,
                               phone = p.phone,
                               address = p.address,
                               start_hour = p.start_hour,
                               end_hour = p.end_hour,
                               working_day = p.working_day,
                               avatar = p.avatar,
                               latitude = (p.latitude != null && p.latitude.Length > 0) ? float.Parse(p.latitude) : float.Parse("0"),
                               longtitude = (p.longtitude != null && p.longtitude.Length > 0) ? float.Parse(p.longtitude) : float.Parse("0"),
                               province_id = p.province_id,
                               province_name = pr != null ? pr.name : "",
                               district_id = p.district_id,
                               ward_id = p.ward_id,
                               service_type_id = p.service_type_id,
                               product_label_id = p.product_label_id,
                               service_type_icons = st != null ? st.icons : null,
                               discount_rate = p.customer_discount_rate,
                               rating = p.rating,
                               is_favourite = true,
                               total_rating = p.total_rating,
                               contract_id = _context.PartnerContracts.Where(x => x.partner_id == p.id && x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12).Select(x => x.id).FirstOrDefault()
                           });

            // Lọc cửa hàng có hợp đồng hiệu lực
            lstData = lstData.Where(x => x.contract_id != null);

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

            if (request.service_type_id != null)
            {
                lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
            }

            if (request.product_label_id != null)
            {
                lstData = lstData.Where(x => x.product_label_id == request.product_label_id);
            }

            List<Guid> list_partner_ids = new List<Guid>();

            if (request.search != null && request.search.Length > 0)
            {
                var lstProducts = (from p in _context.Products
                                   join pg in _context.ProductGroups on p.product_group_id equals pg.id
                                   where p.name.ToLower().Contains(request.search.ToLower()) || pg.name.ToLower().Contains(request.search.ToLower())
                                   select (Guid)p.partner_id).Distinct();

                list_partner_ids = lstProducts.ToList();

                lstData = lstData.Where(x => x.name.ToLower().Contains(request.search.ToLower()) || list_partner_ids.Contains((Guid)x.id));
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

        public APIResponse getListRating(AccumulatePointOrderRatingRequest request)
        {

            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

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
            var partnerObj = _context.Partners.Where(x => x.id == request.partner_id).FirstOrDefault();
            var countRating1 = (from p in _context.AccumulatePointOrderRatings
                                where p.partner_id == request.partner_id && p.rating == 1
                                select p.rating).Count();

            var countRating2 = (from p in _context.AccumulatePointOrderRatings
                                where p.partner_id == request.partner_id && p.rating == 2
                                select p.rating).Count();

            var countRating3 = (from p in _context.AccumulatePointOrderRatings
                                where p.partner_id == request.partner_id && p.rating == 3
                                select p.rating).Count();

            var countRating4 = (from p in _context.AccumulatePointOrderRatings
                                where p.partner_id == request.partner_id && p.rating == 4
                                select p.rating).Count();

            var countRating5 = (from p in _context.AccumulatePointOrderRatings
                                where p.partner_id == request.partner_id && p.rating == 5
                                select p.rating).Count();
            // Khai báo mảng ban đầu
            var lstData = (from p in _context.AccumulatePointOrderRatings
                           join ord in _context.AccumulatePointOrders on p.accumulate_point_order_id equals ord.id
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join c in _context.Customers on p.customer_id equals c.id into cs
                           from c in cs.DefaultIfEmpty()
                           where p.partner_id == request.partner_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = ord.trans_no,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringFull(p.date_created),
                               partner_id = s.id,
                               partner_code = s.code,
                               partner_name = s.name,
                               customer_phone = c.phone,
                               customer_name = c.full_name,
                               content = p.content,
                               rating = p.rating,
                               rating_name = p.rating_name
                           });

            // Đếm số lượng
            int countElements = lstData.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstData.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList, countRating1 = countRating1, countRating2 = countRating2, countRating3 = countRating3, countRating4 = countRating4, countRating5 = countRating5, rating_average = partnerObj.rating != null ? partnerObj.rating : 5 };
            return new APIResponse(dataResult);
        }

        public APIResponse getListProductGroup(Guid partner_id)
        {
            var lstData = (from p in _context.Products
                           join pg in _context.ProductGroups on p.product_group_id equals pg.id
                           where p.partner_id == partner_id
                           select new
                           {
                               id = pg.id,
                               product_group_name = pg.name,
                               product_group_avatr = pg.avatar,
                               partner_id = partner_id
                           }).Distinct().ToList();

            return new APIResponse(lstData);
        }

        public APIResponse getListProduct(ProductRequest request)
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

            var lstData = (from p in _context.Products
                           join pg in _context.ProductGroups on p.product_group_id equals pg.id
                           join bag in _context.PartnerBags.Where(x => x.customer_id == request.customer_id) on p.id equals bag.product_id into bags
                           from bag in bags.DefaultIfEmpty()
                           where p.partner_id == request.partner_id && p.status == 5 && p.status_change == true
                           orderby p.number
                           select new
                           {
                               id = p.id,
                               product_name = p.name,
                               price = p.price,
                               quantity = bag != null ? bag.quantity : 0,
                               description = p.description,
                               product_group_id = pg.id,
                               avatar = p.avatar,
                               partner_id = request.partner_id,
                               status_change = p.status_change,
                               number = p.number,
                               list_images = _context.ProductImages.Where(x => x.id == p.id).ToList()
                           });

            if (request.product_group_id != null)
            {
                lstData = lstData.Where(x => x.product_group_id == request.product_group_id);
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

        public APIResponse getDetailProduct(Guid product_id, Guid customer_id)
        {
            var objProduct = _context.Products.Where(x => x.id == product_id).FirstOrDefault();

            if (objProduct == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            var data = (from p in _context.Products
                        join pg in _context.ProductGroups on p.product_group_id equals pg.id
                        join pa in _context.Partners on p.partner_id equals pa.id
                        join bag in _context.PartnerBags.Where(x => x.customer_id == customer_id) on p.id equals bag.product_id into bags
                        from bag in bags.DefaultIfEmpty()
                        where p.id == product_id
                        select new
                        {
                            id = p.id,
                            product_name = p.name,
                            price = p.price,
                            description = p.description,
                            detail_info = p.detail_info,
                            product_group_id = pg.id,
                            avatar = p.avatar,
                            quantity = bag != null ? bag.quantity : 0,
                            partner_id = p.partner_id,
                            list_images = _context.ProductImages.Where(x => x.id == p.id).ToList(),
                            list_other_products = (from p2 in _context.Products
                                                   join pa2 in _context.Partners on p2.partner_id equals pa2.id
                                                   where p2.product_group_id == p.product_group_id && p2.id != p.id && p2.partner_id == pa.id
                                                   select new
                                                   {
                                                       id = p2.id,
                                                       product_name = p2.name,
                                                       price = p2.price,
                                                       description = p2.description,
                                                       avatar = p2.avatar,
                                                       partner_id = p2.partner_id,
                                                       partner_name = pa2.name
                                                   }).Take(5).ToList()
                        }).FirstOrDefault();

            return new APIResponse(data);
        }

        public APIResponse getVersionByPlatform(string platform)
        {
            var value = _context.AppVersions.Where(x => x.platform == platform.ToUpper() && x.is_active == true).Select(x => x).OrderByDescending(x => x.apply_date).FirstOrDefault();

            if (value == null)
            {
                return new APIResponse("ERROR_PLATFORM_INCORRECT");
            }

            var dataReturn = new
            {
                value = value
            };
            return new APIResponse(dataReturn);
        }

        public async Task<APIResponse> getListPartnerV2(PartnerFilterRequest request, string username)
        {
            try
            {
                var customerId = _context.Users.Where(x => x.username == username && x.is_customer == true).Select(x => x.customer_id).FirstOrDefault();
                decimal generalCustomerRate = 0;

                var generalId = _context.AccumulatePointConfigs.Where(x => x.code == "GENERAL").Select(x => x.id).FirstOrDefault();

                var listAccuDetails = _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == generalId).Select(x => new
                {
                    allocation_name = x.allocation_name,
                    discount_rate = x.discount_rate
                }).ToList();

                for (int j = 0; j < listAccuDetails.Count; j++)
                {
                    if (listAccuDetails[j].allocation_name == "Khách hàng")
                    {
                        generalCustomerRate = (decimal)listAccuDetails[j].discount_rate;
                    }
                }
                var startLongtitude = request.south_west.longtitude;
                var endLongtitude = request.north_east.longtitude;
                var startLatitude = request.south_west.latitude;
                var endLatitude = request.north_east.latitude;

                var lstData = (from p in _context.Partners
                               join f in _context.PartnerFavourites.Where(p => p.customer_id == customerId) on p.id equals f.partner_id into fs
                               from f in fs.DefaultIfEmpty()
                               join con in _context.PartnerContracts.Where(x => x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12) on p.id equals con.partner_id into cons
                               from con in cons.DefaultIfEmpty()
                               join pr in _context.Provinces on p.province_id equals pr.id into prs
                               from pr in prs.DefaultIfEmpty()
                               join di in _context.Provinces on p.district_id equals di.id into dis
                               from di in dis.DefaultIfEmpty()
                               join wa in _context.Provinces on p.ward_id equals wa.id into was
                               from wa in was.DefaultIfEmpty()
                               join st in _context.ServiceTypes on p.service_type_id equals st.id into sts
                               from st in sts.DefaultIfEmpty()
                               where p.status == 15
                               select new PartnerHomeResponse
                               {
                                   id = p.id,
                                   name = p.name,
                                   code = p.code,
                                   phone = p.phone,
                                   address = p.address,
                                   start_hour = p.start_hour,
                                   end_hour = p.end_hour,
                                   working_day = p.working_day,
                                   avatar = p.avatar,
                                   latitude = (p.latitude != null && p.latitude.Length > 0) ? Convert.ToDouble(p.latitude) : 0D,
                                   longtitude = (p.longtitude != null && p.longtitude.Length > 0) ? Convert.ToDouble(p.longtitude) : 0D,
                                   province_id = p.province_id,
                                   province_name = pr.name,
                                   district_id = p.district_id,
                                   ward_id = p.ward_id,
                                   service_type_id = p.service_type_id,
                                   product_label_id = p.product_label_id,
                                   service_type_icons = st != null ? st.icons : null,
                                   is_favourite = f != null ? true : false,
                                   discount_rate = p.customer_discount_rate,
                                   rating = p.rating,
                                   contract_discount_rate = con != null ? con.discount_rate : 0,
                                   total_rating = p.total_rating,
                                   is_hide_discount = false,
                                   contract_id = con != null ? con.id : null
                               });

                // Đếm số lượng
                int countElements = await lstData.CountAsync();

                lstData = lstData.Where(p => p.latitude != null && p.latitude > 0 && startLatitude <= p.latitude && p.latitude <= endLatitude);
                lstData = lstData.Where(p => p.longtitude != null && p.longtitude > 0 && startLongtitude <= p.longtitude && p.longtitude <= endLongtitude);
                if (request.product_group_id != null)
                {
                    var lstPartner = await (from p in _context.Products
                                            where p.product_group_id == request.product_group_id
                                            group p by new
                                            {
                                                p.partner_id,
                                            } into e
                                            select new
                                            {
                                                e.Key.partner_id,
                                            }).ToListAsync();
                    List<Guid> lstPartners = new List<Guid>();
                    foreach (var item in lstPartner)
                    {
                        lstPartners.Add(item.partner_id.Value);
                    }

                    lstData = (from p in _context.Partners
                               join f in _context.PartnerFavourites.Where(x => x.customer_id == customerId) on p.id equals f.partner_id into fs
                               from f in fs.DefaultIfEmpty()
                               join con in _context.PartnerContracts.Where(x => x.from_date <= DateTime.Now && x.to_date >= DateTime.Now && x.status == 12) on p.id equals con.partner_id into cons
                               from con in cons.DefaultIfEmpty()
                               join pr in _context.Provinces on p.province_id equals pr.id into prs
                               from pr in prs.DefaultIfEmpty()
                               join di in _context.Provinces on p.district_id equals di.id into dis
                               from di in dis.DefaultIfEmpty()
                               join wa in _context.Provinces on p.ward_id equals wa.id into was
                               from wa in was.DefaultIfEmpty()
                               join st in _context.ServiceTypes on p.service_type_id equals st.id into sts
                               from st in sts.DefaultIfEmpty()
                               where p.status == 15
                               && (lstPartners.Contains(p.id.Value))
                               orderby p.rating descending
                               select new PartnerHomeResponse
                               {
                                   id = p.id,
                                   name = p.name,
                                   code = p.code,
                                   phone = p.phone,
                                   address = p.address,
                                   start_hour = p.start_hour,
                                   end_hour = p.end_hour,
                                   working_day = p.working_day,
                                   avatar = p.avatar,
                                   latitude = (p.latitude != null && p.latitude.Length > 0) ? float.Parse(p.latitude) : float.Parse("0"),
                                   longtitude = (p.longtitude != null && p.longtitude.Length > 0) ? float.Parse(p.longtitude) : float.Parse("0"),
                                   province_id = p.province_id,
                                   province_name = pr.name,
                                   district_id = p.district_id,
                                   ward_id = p.ward_id,
                                   service_type_id = p.service_type_id,
                                   product_label_id = p.product_label_id,
                                   service_type_icons = st != null ? st.icons : null,
                                   is_favourite = f != null ? true : false,
                                   contract_discount_rate = con.discount_rate != null ? con.discount_rate : 0,
                                   discount_rate = p.customer_discount_rate,
                                   rating = p.rating,
                                   total_rating = p.total_rating,
                                   is_hide_discount = false,
                                   contract_id = con != null ? con.id : null
                               });
                }

                // Lọc cửa hàng có hợp đồng hiệu lực
                lstData = lstData.Where(x => x.contract_id != null);

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

                if (request.service_type_id != null)
                {
                    lstData = lstData.Where(x => x.service_type_id == request.service_type_id);
                }

                if (request.product_label_id != null)
                {
                    lstData = lstData.Where(x => x.product_label_id == request.product_label_id);
                }

                List<Guid> list_partner_ids = new List<Guid>();

                if (request.search != null && request.search.Length > 0)
                {
                    var lstProducts = (from p in _context.Products
                                       join pg in _context.ProductGroups on p.product_group_id equals pg.id
                                       where p.name.ToLower().Contains(request.search.ToLower()) || pg.name.ToLower().Contains(request.search.ToLower())
                                       select (Guid)p.partner_id).Distinct();

                    list_partner_ids = await lstProducts.ToListAsync();

                    lstData = lstData.Where(x => x.name.ToLower().Contains(request.search.ToLower()) || list_partner_ids.Contains((Guid)x.id));
                }

                double zoom = 0;
                var dataList = await lstData
                    .OrderBy(p => Math.Abs((p.latitude ?? 0) - request.latitude))
                    .ThenBy(p => Math.Abs((p.longtitude ?? 0) - request.longtitude))
                    .ThenByDescending(p => p.discount_rate)
                    .ThenBy(p => p.is_favourite)
                    .Distinct()
                    .Take(16)
                    .ToListAsync();
                var currentIds = dataList.Select(x => x.id);
                dataList.ForEach(x => x.is_priority = true);
                dataList.AddRange(await lstData
                    .Where(x => !currentIds.Contains(x.id))
                    .OrderBy(p => Math.Abs((p.latitude ?? 0) - request.latitude))
                    .ThenBy(p => Math.Abs((p.longtitude ?? 0) - request.longtitude))
                    .ThenBy(p => p.discount_rate)
                    .ThenByDescending(p => p.is_favourite)
                    .Distinct()
                    .Take(16)
                    .ToListAsync());

                var dateNow = DateTime.Now;

                for (int i = 0; i < dataList.Count; i++)
                {
                    decimal customerExchange = 0;

                    // Lấy cấu hình đổi điểm hiệu lực
                    var accumulateConfigId = await _context.AccumulatePointConfigs.Where(x => x.code == null && x.from_date <= dateNow && x.to_date >= dateNow && x.partner_id == dataList[i].id && x.status == 23).Select(x => x.id).FirstOrDefaultAsync();

                    // Nếu không có riêng thì lấy chung
                    if (accumulateConfigId != null)
                    {
                        var listAccuDetail2s = await _context.AccumulatePointConfigDetails.Where(x => x.accumulate_point_config_id == accumulateConfigId).Select(x => new
                        {
                            allocation_name = x.allocation_name,
                            discount_rate = x.discount_rate
                        }).ToListAsync();


                        for (int j = 0; j < listAccuDetail2s.Count; j++)
                        {
                            if (listAccuDetail2s[j].allocation_name == "Khách hàng")
                            {
                                customerExchange = (decimal)listAccuDetail2s[j].discount_rate;
                            }
                        }
                        dataList[i].discount_rate = Math.Round((decimal)dataList[i].contract_discount_rate * customerExchange / 10) / 10;
                    }
                    else
                    {
                        if (dataList[i].contract_discount_rate == null)
                        {
                            dataList[i].contract_discount_rate = 0;
                        }
                        dataList[i].discount_rate = Math.Round((decimal)dataList[i].contract_discount_rate * generalCustomerRate / 10) / 10;
                    }
                    dataList[i].distance = CalculateDistanceV2(request.latitude, request.longtitude, dataList[i].latitude ?? 0, dataList[i].longtitude ?? 0);
                    if (dataList[i].distance < 3000 || dataList[i].is_favourite == true)
                    {
                        dataList[i].is_hide_discount = false;
                    }
                    else
                    {
                        dataList[i].is_hide_discount = true;
                    }
                }

                var maxDistance = dataList.Max(x => x.distance);
                var distance = CalculateDistanceV2(request.latitude, request.longtitude, request.south_west.latitude, request.south_west.longtitude);
                if (dataList.Count < 32 && request.is_update_zoom)
                {
                    if (distance > 200000)
                    {
                        zoom = 0;
                    }
                    else if (distance >= 150000)
                    {
                        zoom = 2.6;
                    }
                    else if (distance >= 100000)
                    {
                        zoom = 2;
                    }
                    else if (distance >= 70000)
                    {
                        zoom = 1.4;
                    }
                    else if (distance >= 50000)
                    {
                        zoom = 1;
                    }
                    else if (distance >= 30000)
                    {
                        zoom = 0.8;
                    }
                    else if (distance >= 10000)
                    {
                        zoom = 0.5;
                    }
                    else if (distance >= 5000)
                    {
                        zoom = 0.15;
                    }
                    else if (distance >= 1000)
                    {
                        zoom = 0.07;
                    }
                    else if (distance < 1000)
                    {
                        zoom = 0.04;
                    }
                }

                var dataResult = new { total_elements = countElements, zoom = zoom, maxDistance = distance, data = dataList };
                return new APIResponse(dataResult);
            }
            catch (Exception ex)
            {
                return new APIResponse(ex);
            }

        }
    }
}
