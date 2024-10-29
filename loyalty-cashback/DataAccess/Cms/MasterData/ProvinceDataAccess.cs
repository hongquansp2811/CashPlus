using System;
using System.Linq;
using LOYALTY.Interfaces;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using LOYALTY.Data;
using LOYALTY.Models;

namespace LOYALTY.DataAccess
{
    public class ProvinceDataAccess : IProvince
    {
        private readonly LOYALTYContext _context;
        public ProvinceDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(ProvinceRequest request)
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
            var lstProvince = (from p in _context.Provinces
                               join p2 in _context.Provinces on p.parent_id equals p2.id into p2s
                               from p2 in p2s.DefaultIfEmpty()
                               join p3 in _context.Provinces on p2.parent_id equals p3.id into p3s
                               from p3 in p3s.DefaultIfEmpty()
                               where p.types == request.types
                               orderby p.date_created descending
                               select new
                               {
                                   id = p.id,
                                   code = p.code,
                                   name = p.name,
                                   types = p.types,
                                   description = p.description,
                                   district_id = request.types == 3 && p2 != null ? p2.id : null,
                                   district_name = request.types == 3 && p2 != null ? p2.name : null,
                                   province_id = request.types == 2 && p2 != null ? p2.id : (request.types == 3 && p3 != null ? p3.id : null),
                                   province_name = request.types == 2 && p2 != null ? p2.name : (request.types == 3 && p3 != null ? p3.name : null)
                               });

            // Nếu tồn tại Where theo tên
            if (request.name != null && request.name.Length > 0)
            {
                lstProvince = lstProvince.Where(x => x.code.Trim().ToLower().Contains(request.name.Trim().ToLower()) || x.name.Trim().ToLower().Contains(request.name.Trim().ToLower()));
            }

            if (request.province_id != null)
            {
                lstProvince = lstProvince.Where(x => x.province_id == request.province_id);
            }

            if (request.district_id != null)
            {
                lstProvince = lstProvince.Where(x => x.district_id == request.district_id);
            }
            // Đếm số lượng
            int countElements = lstProvince.Count();

            // Số lượng trang
            int totalPage = countElements > 0
                    ? (int)Math.Ceiling(countElements / (double)request.page_size)
                    : 0;

            // Data Sau phân trang
            var dataList = lstProvince.Take(request.page_size * request.page_no).Skip(skipElements).ToList();
            var dataResult = new DataListResponse { page_no = request.page_no, page_size = request.page_size, total_elements = countElements, total_page = totalPage, data = dataList };
            return new APIResponse(dataResult);
        }

        public APIResponse getDetail(int id)
        {
            var data = (from p in _context.Provinces
                        join p2 in _context.Provinces on p.parent_id equals p2.id into p2s
                        from p2 in p2s.DefaultIfEmpty()
                        join p3 in _context.Provinces on p2.parent_id equals p3.id into p3s
                        from p3 in p3s.DefaultIfEmpty()
                        where p.id == id
                        select new
                        {
                            id = p.id,
                            code = p.code,
                            name = p.name,
                            types = p.types,
                            description = p.description,
                            district_id = p.types == 3 && p2 != null ? p2.id : null,
                            district_name = p.types == 3 && p2 != null ? p2.name : null,
                            province_id = p.types == 2 && p2 != null ? p2.id : (p.types == 3 && p3 != null ? p3.id : null),
                            province_name = p.types == 2 && p2 != null ? p2.name : (p.types == 3 && p3 != null ? p3.name : null)
                        }).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }
            return new APIResponse(data);
        }

        public APIResponse create(ProvinceRequest request, string username)
        {

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            try
            {
                var code = "";
                if (request.types == 1)
                {
                    var maxCodeObject = _context.Provinces.Where(x => x.types == 1 && x.code != null && x.code.Contains("TT")).OrderByDescending(x => x.code).FirstOrDefault();
                    if (maxCodeObject == null)
                    {
                        code = "TT001";
                    }
                    else
                    {
                        string maxCode = maxCodeObject.code;
                        maxCode = maxCode.Substring(2);
                        int orders = int.Parse(maxCode);
                        orders = orders + 1;
                        string orderString = orders.ToString();
                        char pad = '0';
                        int number = 3;
                        code = "TT" + orderString.PadLeft(number, pad);
                    }
                } else if (request.types == 2)
                {
                    var maxCodeObject = _context.Provinces.Where(x => x.types == 2 && x.code != null && x.code.Contains("QH")).OrderByDescending(x => x.code).FirstOrDefault();
                    if (maxCodeObject == null)
                    {
                        code = "QH000001";
                    }
                    else
                    {
                        string maxCode = maxCodeObject.code;
                        maxCode = maxCode.Substring(2);
                        int orders = int.Parse(maxCode);
                        orders = orders + 1;
                        string orderString = orders.ToString();
                        char pad = '0';
                        int number = 6;
                        code = "QH" + orderString.PadLeft(number, pad);
                    }
                } else if (request.types == 3)
                {
                    var maxCodeObject = _context.Provinces.Where(x => x.types == 3 && x.code != null && x.code.Contains("XP")).OrderByDescending(x => x.code).FirstOrDefault();
                    if (maxCodeObject == null)
                    {
                        code = "XP000001";
                    }
                    else
                    {
                        string maxCode = maxCodeObject.code;
                        maxCode = maxCode.Substring(2);
                        int orders = int.Parse(maxCode);
                        orders = orders + 1;
                        string orderString = orders.ToString();
                        char pad = '0';
                        int number = 6;
                        code = "XP" + orderString.PadLeft(number, pad);
                    }
                }
                var data = new Province();
                var oldId = (from p in _context.Provinces
                             orderby p.id descending
                             select p.id
                              ).FirstOrDefault();
                data.id = oldId == null ? 1 : (oldId + 1);
                data.code = code;
                data.name = request.name;
                data.parent_id = request.parent_id;
                data.types = request.types;
                data.description = request.description;
                data.user_created = username;
                data.date_created = DateTime.Now;
                data.user_updated = username;
                data.date_updated = DateTime.Now;
                _context.Provinces.Add(data);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }

        public APIResponse update(ProvinceRequest request, string username)
        {
            if (request.id == null)
            {
                return new APIResponse("ERROR_ID_MISSING");
            }
            var data = _context.Provinces.Where(x => x.id == request.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            if (request.name == null)
            {
                return new APIResponse("ERROR_NAME_MISSING");
            }

            try
            {
                data.name = request.name;
                data.parent_id = request.parent_id;
                data.types = request.types;
                data.description = request.description;
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
            var data = _context.Provinces.Where(x => x.id == req.id).FirstOrDefault();
            if (data == null)
            {
                return new APIResponse("ERROR_ID_NOT_EXISTS");
            }

            try
            {
                _context.Provinces.Remove(data);
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
