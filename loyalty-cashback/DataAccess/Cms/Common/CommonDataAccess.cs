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
    public class CommonDataAccess : ICommon
    {
        private readonly LOYALTYContext _context;
        public CommonDataAccess(LOYALTYContext context) 
        {
            this._context = context;
        }

        public bool checkUpdateCustomerRank(Guid user_id)
        {
            bool returnData = true;

            var userObj = _context.Users.Where(x => x.id == user_id && x.is_customer == true).Select(x => new { point_avaiable = x.point_avaiable, customer_id = x.customer_id }).FirstOrDefault();

            if (userObj == null)
            {
                returnData = false;
            } else
            {
                var customerObj = _context.Customers.Where(x => x.id == userObj.customer_id).FirstOrDefault();
                var customerRankConfigObj = _context.CustomerRankConfigs.Where(x => x.condition_upgrade <= userObj.point_avaiable).OrderByDescending(x => x.condition_upgrade).FirstOrDefault();

                if (customerRankConfigObj != null)
                {
                    customerObj.customer_rank_id = customerRankConfigObj.customer_rank_id;
                } else {
                    customerObj.customer_rank_id = Guid.Parse("26C4F206-6B1C-4702-851C-8BAC40CFC2BB");
                }
                _context.SaveChanges();
            }

            return returnData;
        }
    }
}
