using System;

namespace DataObjects.Response
{
    public class UserReportRes
    {
        public Guid? id  { get; set; }   
        public DateTime?  Date_join { get; set; }  //Ngày tham gia
        public string? bank_acc { get; set; } //Tài khoản 
        public string? name { get; set; } //Tên
        public decimal? point_use { get; set; } //Điểm tiêu dùng
        public decimal? point_save { get; set; } //Điểm tích lũy
        public int? count_introduce { get; set; }    //Tổng số giới thiệu
        public int? count_transaction_completed { get; set; } //Tổng số giao dịch đã hoàn thành
        public decimal? money_refund { get; set; } //tổng số tiền được hoàn
        public int? Evaluate { get; set; } //Đánh giá
        public int? Complain { get; set; } //Khiếu nại
        public int? count_transaction { get; set; } //Tổng giao dịch
        public decimal? total_changed { get; set; } //Tổng điểm đã đổi
        public Guid? share_code { get; set; }
    }
}
