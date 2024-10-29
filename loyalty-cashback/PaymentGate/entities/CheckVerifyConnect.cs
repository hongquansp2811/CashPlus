using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace LOYALTY.PaymentGate
{
    public class CheckVerifyConnect
    {
        public CheckVerifyConnect() { }
        private int _ResponseCode;
        private string _ResponseMessage;
        private SubResponseCode _SubResponseCode;
        private string _ReferenceId;
        private string _TransactionId;
        private string _TransactionTime;
        private string _BankNo;
        private string _AccNo;
        private string _AccName;
        private string _AccType;
        private int? _Status;
        private string _Signature;
        private string? _Reason;
        private string? _Accname;
        public int ResponseCode
        {
            get { return _ResponseCode; }
            set { _ResponseCode = value; }
        }
        public string ResponseMessage
        {
            get { return _ResponseMessage; }
            set { _ResponseMessage = value; }
        }
        public string ReferenceId
        {
            get { return _ReferenceId; }
            set { _ReferenceId = value; }
        }
        public string TransactionId
        {
            get { return _TransactionId; }
            set { _TransactionId = value; }
        }
        public string TransactionTime
        {
            get { return _TransactionTime; }
            set { _TransactionTime = value; }
        }
        public string BankNo
        {
            get { return _BankNo; }
            set { _BankNo = value; }
        }
        public string AccNo
        {
            get { return _AccNo; }
            set { _AccNo = value; }
        }
        public string AccName
        {
            get { return _AccName; }
            set { _AccName = value; }
        }
        public int? Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        public string AccType
        {
            get { return _AccType; }
            set { _AccType = value; }
        }
        public string Accname
        {
            get { return _Accname; }
            set { _Accname = value; }
        }
        public string Signature
        {
            get { return _Signature; }
            set { _Signature = value; }
        }

        public string? Reason
        {
            get { return _Reason; }
            set { _Reason = value; }
        }
        public SubResponseCode SubResponseCode
        {
            get { return _SubResponseCode; }
            set { _SubResponseCode = value; }
        }

        public override string ToString()
        {
            var option = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, option);
        }
    }
}
