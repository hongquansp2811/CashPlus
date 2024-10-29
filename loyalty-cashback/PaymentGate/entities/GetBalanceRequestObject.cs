using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.PaymentGate
{
    public class GetBalanceRequestObject
    {
            public GetBalanceRequestObject()
            {

            }

            private string _RequestId = "";
            private string _RequestTime = "";
            private string _PartnerCode = "";
            private int _Operation = 9004;
            private string _Signature = "";
            public string RequestId
            {
                get
                {
                    return _RequestId;
                }
                set
                {
                    _RequestId = value;
                }
            }
            public string RequestTime
            {
                get
                {
                    return _RequestTime;
                }
                set
                {
                    _RequestTime = value;
                }
            }
            public string PartnerCode
            {
                get
                {
                    return _PartnerCode;
                }
                set
                {
                    _PartnerCode = value;
                }
            }
            public int Operation
            {
                get
                {
                    return _Operation;
                }
                set
                {
                    _Operation = value;
                }
            }
          
            public string Signature
            {
                get
                {
                    return _Signature;
                }
                set
                {
                    _Signature = value;
                }
            }
    }
}
