namespace LOYALTY.PaymentGate
{
    public class BKFirmBankResponse
    {
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string RequestId { get; set; }
        public string BankNo { get; set; }
        public string AccNo { get; set; }
        public string AccType { get; set; }
        public string AccName { get; set; }
        public string Signature { get; set; }
    }
}
