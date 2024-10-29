using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace LOYALTY.PaymentGate
{
    public class GetBalanceResponseObj
    {
        public GetBalanceResponseObj() { }
        private int _ResponseCode;
        private string _ResponseMessage;
        private string _RequestId;
        private string _PartnerCode;
        private long _Available;
        private long _Holding;
        private string _Signature;
        private string _body;
        private GetBalanceRequestObject _bodyRequest;

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

        public string RequestId
        {
            get { return _RequestId; }
            set { _RequestId = value; }
        }

        public string PartnerCode
        {
            get { return _PartnerCode; }
            set { _PartnerCode = value; }
        }
        public long Available
        {
            get { return _Available; }
            set { _Available = value; }
        }
        public long Holding
        {
            get { return _Holding; }
            set { _Holding = value; }
        }
        public string Signature
        {
            get { return _Signature; }
            set { _Signature = value; }
        }
        public string body
        {
            get { return _body; }
            set { _body = value; }
        }
        public GetBalanceRequestObject bodyRequest
        {
            get { return _bodyRequest; }
            set { _bodyRequest = value; }
        }

        public override string ToString()
        {
            var option = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, option);
        }
    }
}
