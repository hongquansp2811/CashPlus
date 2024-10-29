using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;
using System;
using System.Threading.Tasks;
using static LOYALTY.DataAccess.AppCustomerHomeDataAccess;

namespace LOYALTY.Interfaces
{
    public interface IAppCustomerHome
    {
        // API lấy thông tin trang chủ
        public APIResponse getHomeInfo(Guid customer_id);

        // API lấy thông tin chung
        public APIResponse getHomeGeneralInfo();
        public APIResponse getPointByTime(reqPoint request);
        public APIResponse getSuggestSearch(PartnerRequest request);
        // API danh sách cửa hàng dạng danh sách
        public Task<APIResponse> getListPartner(PartnerRequest request, string username);
        public APIResponse likePartner(PartnerFavourite request, string username);
        public APIResponse dislikePartner(PartnerFavourite request, string username);
        public APIResponse getListPartnerFavourite(PartnerRequest request, string username);
        // API Danh sách đánh giá
        public APIResponse getListRating(AccumulatePointOrderRatingRequest request);
        // API danh sách nhóm sản phẩm
        public APIResponse getListProductGroup(Guid partner_id);
        // API Danh sách sản phẩm
        public APIResponse getListProduct(ProductRequest request);
        // API Chi tiết sản phẩm
        public APIResponse getDetailProduct(Guid product_id, Guid customer_id);
        public APIResponse getTotalNotificationNotRead(Guid customer_id);

        public APIResponse getVersionByPlatform(string platform);

        public Task<APIResponse> getListPartnerV2(PartnerFilterRequest request, string username);

    }
}
