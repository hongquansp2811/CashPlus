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
    public class AppPartnerBagDataAccess : IAppPartnerBag
    {
        private readonly LOYALTYContext _context;
        public AppPartnerBagDataAccess(LOYALTYContext context)
        {
            this._context = context;
        }

        public APIResponse getList(Guid customer_id)
        {
            var partnerIds = (from p in _context.PartnerBags
                              join s in _context.Partners on p.partner_id equals s.id
                              where p.customer_id == customer_id
                              select p.partner_id).Distinct().ToList();

            var lstData = (from p in _context.Partners
                           where partnerIds.Contains(p.id)
                           select new
                           {
                               partner_id = p.id,
                               partner_name = p.name,
                               discount_rate = p.customer_discount_rate,
                               list_item = (from b in _context.PartnerBags
                                            join pr in _context.Products on b.product_id equals pr.id
                                            where b.partner_id == b.partner_id && b.customer_id == customer_id
                                            select new
                                            {
                                                id = b.id,
                                                product_id = pr.id,
                                                product_name = pr.name,
                                                description = pr.description,
                                                discount_rate = p.customer_discount_rate,
                                                avatar = pr.avatar,
                                                price = pr.price,
                                                quantity = b.quantity,
                                                total_amount = b.total_amount
                                            }).ToList()
                           });
            return new APIResponse(lstData);
        }

        public APIResponse getDetailByPartnerId(PartnerBagRequest request)
        {
            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_ID_MISSING");
            }

            var data = (from p in _context.PartnerBags
                        join pr in _context.Products on p.product_id equals pr.id
                        join s in _context.Partners on p.partner_id equals s.id
                        where p.partner_id == request.partner_id
                        select new
                        {
                            id = p.id,
                            product_id = pr.id,
                            product_name = pr.name,
                            discount_rate = s.customer_discount_rate,
                            price = pr.price,
                            description = pr.description,
                            avatar = pr.avatar,
                            partner_id = p.partner_id,
                            partner_name = s.name,
                            quantity = p.quantity,
                            total_amount = p.total_amount
                        }).ToList();

            return new APIResponse(data);
        }

        public APIResponse create(PartnerBagRequest request, string username)
        {
            if (request.partner_id == null)
            {
                return new APIResponse("ERROR_PARTNER_MISSING");
            }

            if (request.customer_id == null)
            {
                return new APIResponse("ERROR_CUSTOMER_MISSING");
            }

            if (request.product_id == null)
            {
                return new APIResponse("ERROR_USER_ID_MISSING");
            }

            if (request.quantity == null)
            {
                return new APIResponse("ERROR_QUANTITY_MISSING");
            }

            var data = _context.PartnerBags.Where(x => x.partner_id == request.partner_id && x.customer_id == request.customer_id && x.product_id == request.product_id).FirstOrDefault();
            bool isNew = false;
            bool isDelete = false;
            if (request.quantity == 0)
            {
                isDelete = true;
            }
            if (data == null)
            {
                isNew = true;
                data = new PartnerBag();
                data.id = Guid.NewGuid();
            }

            var dataProduct = _context.Products.Where(x => x.id == request.product_id).FirstOrDefault();

            try
            {
                if (isDelete == true)
                {
                    var data2 = _context.PartnerBags.Where(x => x.partner_id == request.partner_id && x.customer_id == request.customer_id && x.product_id == request.product_id).FirstOrDefault();

                    if (data2 != null)
                    {
                        _context.PartnerBags.Remove(data);
                        _context.SaveChanges();
                    }
                }
                else
                {
                    data.partner_id = request.partner_id;
                    data.customer_id = request.customer_id;
                    data.product_id = request.product_id;
                    data.quantity = request.quantity;
                    data.total_amount = (data.quantity * dataProduct.price);
                    data.user_updated = username;
                    data.date_updated = DateTime.Now;
                    if (isNew == true)
                    {
                        data.user_created = username;
                        data.date_created = DateTime.Now;
                        _context.PartnerBags.Add(data);
                    }
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return new APIResponse("ERROR_ADD_FAIL");
            }

            return new APIResponse(200);
        }
    }
}
