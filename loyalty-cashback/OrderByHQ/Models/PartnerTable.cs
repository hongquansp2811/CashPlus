using LOYALTY.Extensions;
using System;
using System.ComponentModel;

namespace LOYALTY.OrderByHQ.Models
{
    public class PartnerTable
    {
        public Guid id { get; set; }
        public Guid partner_id { get; set; }
        public int? partner_branch_id { get; set; }
        public string? name { get; set; }
        public int floor { get; set; }
        public int? capacity { get; set; }
        public string? area { get; set; }
        public Enums.TableStatus? status { get; set; }

    }

    public class Enums
    {
        public enum TableStatus
        {
            Free = 1,
            Busy = 2,
            Maintain = 3,
        }
        public enum ActionType
        {
            View = 1,
            Add = 2,
            Edit = 3,
            Delete = 4,
            Export = 5,
            Import = 6,
            Other = 7
        }

        public enum SmsProviderId
        {
            ESms = 1,
            BlueLink = 2
        }

        public enum ReconciliationType
        {
            Income = 1,
            Expenditure = 2
        }

        public enum NotificationGetListTarget
        {
            User = 1,
            Merchant = 2
        }

        public enum PaymentType
        {
            Cash = 1,
            BaoKim = 2
        }

        public enum BkTransactionType
        {
            [Description("MER_TRANSFER_NTD")] ToUser = 1,
            [Description("PAYMENT_GATE")] ToPaymentGate = 2,
            [Description("AFFILIATE")] ToAffiliate = 3,
            [Description("MER_TRANSFER_SYS")] ToSystem = 4,
            [Description("SYS_TRANSFER_MERCHANT")] ToMerchant = 5
        }

        public enum BkTransactionStatus
        {
            Success = 25,
            Failed = 26,
            Pending = 27
        }

        public static class FeePayer
        {
            public const string USER = "USER";
            public const string MERCHANT = "MERCHANT";
            public const string ATS = "ATS";
            public const string BAOKIM = "BAOKIM";
        }

        public enum OrderType
        {
            [Description("CHANGE_POINT")] CHANGE_POINT,
            [Description("PUSH")] PUSH,
            [Description("AFF_LV_1")] AFF_LV_1,
        }
    }
}
