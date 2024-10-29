using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOYALTY.Models
{
    public class NotiConfig : MasterCommonModel
    {
        public Guid? id { get; set; }
        public string? Payment_NotEnRefund { get; set; } //Thanh toán thành công không đủ đk hoàn tiền
        public string? Payment_Refund { get; set; } //Thanh toán thành công  đủ đk hoàn tiền
        public string? Payment_RefundFail { get; set; } //Thanh toán thất bại
        public string? Payment_CheckSurplus { get; set; } //Thanh toán - Check số dư
        public string? Payment_SMSPointSave { get; set; } //Thanh toán - SMS điểm tiêu dùng
        public string? Payment_SMSPointUse { get; set; } //Thanh toán - SMS điểm tích lũy
        public string? ChangePoint_Add { get; set; } //Đổi điểm - Thêm mới  
        public string? ChangePoint_Acp { get; set; } //Đổi điểm - Duyệt  
        public string? ChangePoint_De { get; set; } //Đổi điểm - Hủy 

        public string? MC_Payment_Refund { get; set; } //Hoàn tiền tiêu dùng
        public string? MC_Payment_RefundFail { get; set; } //Thanh toán thất bại

        public string? MC_Payment_CheckSurplus { get; set; } // Check số dư Merchant

        public string? MC_Payment_SMSPointSave { get; set; } //Thanh toán - SMS điểm tích lũy Merchant
        public string? MC_Payment_SMSPointUse { get; set; } //Thanh toán - SMS điểm tiêu dùng Merchant 


        public string? MC_Payment_Rating { get; set; } // NTD đánh giá giao dịch

        public string? Product_Acp { get; set; } // Product - Duyệt  
        public string? Product_De { get; set; } // Product - Hủy 

        public string? MC_ChangePoint_Add { get; set; } //Đổi điểm - Thêm mới  Merchant
        public string? MC_ChangePoint_Acp { get; set; } //Đổi điểm - Duyệt  Merchant
        public string? MC_ChangePoint_De { get; set; } //Đổi điểm - Hủy Merchant

        public string? MC_amount_bill { get; set; } //merchant - Hạn mức tối thiểu 
    }
}