using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface ISettings
    {
        public APIResponse getDetail();
        public APIResponse update(SettingsRequest request, string username);
        public APIResponse getListHistory(ConfigHistoryRequest request);
        public APIResponse getListCustomerRankConfig();
        public APIResponse getListRatingConfig();
        public APIResponse getListExchangePointPackConfig();
        public APIResponse updateCustomerRankConfig(CustomerRankConfigRequest request, string username);
        public APIResponse updateRatingConfig(RatingConfigRequest request, string username);
        public APIResponse updateExchangePointPackConfig(ExchangePointPackConfigRequest request, string username);
    }
}
