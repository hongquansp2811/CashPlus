using System;

namespace DataObjects.Response
{
    public class PartnerReportRes
    {
        public Guid? partner_id { get; set; }
        public DateTime? Date_join { get; set; }  //Ngày tham gia
        public string? Code { get; set; } //Mã 
        public string? name { get; set; } //Tên
        public decimal? discount_percentage { get; set; } //% chiết khấu
        public int? count_introduce { get; set; }    //Tổng số giới thiệu
        public decimal? point_save { get; set; } //Điểm tích lũy
        public int? count_transaction { get; set; } //Tổng số giao dịch 
        public decimal? total_revenue { get; set; } //Tổng doanh thu
        public decimal? total_discount { get; set; } //tổng chiết khấu
        public int? count_product { get; set; } //tổng số sản phẩm
        public int? count_epl { get; set; } //tổng số nhân viên
        public int? Evaluate { get; set; } //Đánh giá
        public int? Complain { get; set; } //Khiếu nại
        public int? transaction { get; set; } //số lượng giao dịch
        public decimal? total_changed { get; set; } //Tổng điểm đã đổi
        
    }
}
