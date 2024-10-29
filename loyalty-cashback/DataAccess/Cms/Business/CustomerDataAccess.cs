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
    public class CustomerDataAccess : ICustomer
    {
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public CustomerDataAccess(LOYALTYContext context, ICommonFunction commonFunction)
        {
            this._context = context;
            _commonFunction = commonFunction;
        }

        public APIResponse getList(CustomerRequest request)
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
            var lstData = (from p in _context.Customers
                           join u in _context.Users on p.id equals u.customer_id
                           join sp in _context.Users on u.share_person_id equals sp.id into sps
                           from sp in sps.DefaultIfEmpty()
                           join r in _context.CustomerRanks on p.customer_rank_id equals r.id into rs
                           from r in rs.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               phone = p.phone,
                               full_name = p.full_name,
                               share_person = sp != null ? sp.username : "",
                               birth_date = p.birth_date != null ? _commonFunction.convertDateToStringSort(p.birth_date) : "",
                               customer_rank_id = p.customer_rank_id,
                               customer_rank_name = r != null ? r.name : "",
                               status = p.status,
                               status_name = st != null ? st.name : ""
                           });

            // Nếu tồn tại Where theo tên
            if (request.phone != null && request.phone.Length > 0)
            {
                lstData = lstData.Where(x => x.phone.Trim().ToLower().Contains(request.phone.Trim().ToLower()) || x.full_name.Trim().ToLower().Contains(request.phone.Trim().ToLower()));
            }

            if (request.customer_rank_id != null)
            {
                lstData = lstData.Where(x => x.customer_rank_id == request.customer_rank_id);
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
            var settings = _context.Settingses.FirstOrDefault();
            decimal pointExchange = settings != null && settings.point_exchange != null ? (decimal)settings.point_exchange : 1;
            decimal pointValue = settings != null && settings.point_value != null && settings.point_value != 0 ? (decimal)settings.point_value : 1;
            decimal pointExchangeRate = pointExchange / pointValue;
            var data = (from p in _context.Customers
                        join u in _context.Users on p.id equals u.customer_id
                        join sp in _context.Users on u.share_person_id equals sp.id into sps
                        from sp in sps.DefaultIfEmpty()
                        join r in _context.CustomerRanks on p.customer_rank_id equals r.id into rs
                        from r in rs.DefaultIfEmpty()
                        join st in _context.OtherLists on p.status equals st.id into sts
                        from st in sts.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            avatar = p.avatar,
                            phone = p.phone,
                            full_name = p.full_name,
                            email = p.email,
                            tax_tncn = p.tax_tncn,
                            share_code = u.share_code,
                            share_person = sp != null ? sp.username : "",
                            birth_date = p.birth_date != null ? _commonFunction.convertDateToStringSort(p.birth_date) : "",
                            customer_rank_id = p.customer_rank_id,
                            customer_rank_name = r != null ? r.name : "",
                            status = p.status,
                            status_name = st != null ? st.name : "",
                            point_avaiable = u.point_avaiable,
                            point_waiting = u.point_waiting,
                            point_affiliate = u.point_affiliate,
                            total_point = _context.AccumulatePointOrders.Where(x => x.customer_id == p.id && x.approve_date != null && x.point_customer != null && x.return_type == "Cash").Select(x => x.point_customer).Sum(x => x.Value),
                            pointExchangeRate = pointExchangeRate,
                            list_bank_accounts = (from bc in _context.CustomerBankAccounts
                                                  join b in _context.Banks on bc.bank_id equals b.id into bs
                                                  from b in bs.DefaultIfEmpty()
                                                  where bc.user_id == p.id
                                                  select new
                                                  {
                                                      id = bc.id,
                                                      bank_name = b != null ? b.name : "",
                                                      bank_no = bc.bank_no,
                                                      bank_owner = bc.bank_owner
                                                  }).ToList()
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse update(CustomerRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Customers.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                data.full_name = request.full_name;
                data.email = request.email;
                data.tax_tncn = request.tax_tncn;

                if (request.birth_date != null && request.birth_date.Length == 10)
                {
                    data.birth_date = _commonFunction.convertStringSortToDate(request.birth_date);
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse changePassword(PasswordRequest request)
        {
            var data = _context.Users.Where(x => x.customer_id == request.user_id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                data.password = _commonFunction.ComputeSha256Hash(request.new_password);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_UPDATE_FAIL");
            }
            return new APIResponse(200);
        }

        public APIResponse changeStatus(DeleteGuidRequest request)
        {
            var data = _context.Customers.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            var userdata = _context.Users.Where(x => x.customer_id == data.id).FirstOrDefault();
            try
            {
                data.status = request.status_id;
                userdata.status = request.status_id;
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
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               trans_no = p.trans_no,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               partner_code = s.code,
                               partner_name = s.name,
                               address = s.address,
                               bill_amount = p.bill_amount,
                               point_exchange = p.point_exchange,
                               point_customer = p.point_customer,
                               point_partner = p.point_partner,
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
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           join st in _context.OtherLists on p.status equals st.id into sts
                           from st in sts.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           orderby p.date_created descending
                           select new
                           {
                               id = p.id,
                               order_code = p.order_code,
                               trans_date = _commonFunction.convertDateToStringSort(p.date_created),
                               trans_date_origin = p.date_created,
                               total_amount = p.total_amount,
                               status = p.status,
                               partner_name = s.name,
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
                           join s in _context.Partners on p.partner_id equals s.id into ss
                           from s in ss.DefaultIfEmpty()
                           where p.customer_id == request.customer_id
                           select new
                           {
                               id = p.id,
                               trans_no = ord.trans_no,
                               date_created_origin = p.date_created,
                               date_created = _commonFunction.convertDateToStringSort(p.date_created),
                               partner_code = s.code,
                               partner_name = s.name,
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

        public APIResponse getListTeam(CustomerRequest request)
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

            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_ID_MISSING");
            }

            var userObj = _context.Users.Where(x => x.customer_id == request.customer_id).FirstOrDefault();

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
                                     total_point_accumulate = (from tp in _context.CustomerPointHistorys
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
            //                         total_point_accumulate = (from tp in _context.CustomerPointHistorys
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
            //                         total_point_accumulate = (from tp in _context.CustomerPointHistorys
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

            //lstResult = lstResult.Where(x => x.total_point_accumulate > 0);

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
            } else
            {
                lstResult = lstResult.OrderByDescending(x => x.date_created);
            }

            var data_count = lstResult.Count();
            //var values = lstResult.Sum(x => x.total_point_accumulate);

            // Đếm số lượng
            int countElements = lstResult.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstResult.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList, values = 0, data_count = data_count };

            return new APIResponse(dataResult);
        }
    }
}
