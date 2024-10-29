namespace LOYALTY.PaymentGate
{
    public class BKFirmBankinkRequest
    {
        public string RequestId { get; set; }
        public string RequestTime { get; set; }
        public string PartnerCode { get; set; }
        public string Operation { get; set; }
        public string BankNo { get; set; }
        public string AccNo { get; set; }
        public int AccType { get; set; }
        public string Signature { get; set; }
    }
}
