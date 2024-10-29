using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.PaymentGate
{
    public class RequestObject
    {
        public RequestObject()
        {

        }

        private string _RequestId = "";
        private string _RequestTime = "";
        private string _PartnerCode = "";
        private int _Operation = 9002;
        private string _BankNo = "";
        private string _AccNo = "";
        private int _AccType = 0;
        private string _ReferenceId = "";
        private decimal _RequestAmount = 0;
        private string _Memo = "";
        private string _AccountName = "";
        private string _Signature = "";
        private string _Extends = "";
        private string _ContractNumber = "";
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
        public string ReferenceId
        {
            get
            {
                return _ReferenceId;
            }
            set
            {
                _ReferenceId = value;
            }
        }
        public string BankNo
        {
            get
            {
                return _BankNo;
            }
            set
            {
                _BankNo = value;
            }
        }
        public string AccNo
        {
            get
            {
                return _AccNo;
            }
            set
            {
                _AccNo = value;
            }
        }
        public int AccType
        {
            get
            {
                return _AccType;
            }
            set
            {
                _AccType = value;
            }
        }
        public decimal RequestAmount
        {
            get
            {
                return _RequestAmount;
            }
            set
            {
                _RequestAmount = value;
            }
        }
        public string Memo
        {
            get
            {
                return _Memo;
            }
            set
            {
                _Memo = value;
            }
        }
        public string AccountName
        {
            get
            {
                return _AccountName;
            }
            set
            {
                _AccountName = value;
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
        public string Extends
        {
            get
            {
                return _Extends;
            }
            set
            {
                _Extends = value;
            }
        }
        public string ContactNumber
        {
            get
            {
                return _ContractNumber;
            }
            set
            {
                _ContractNumber = value;
            }
        }
    }
    public class RequestObjectVerify
    {
        public RequestObjectVerify()
        {

        }

        private string _RequestId = "";
        private string _RequestTime = "";
        private string _PartnerCode = "";
        private int _Operation = 9001;
        private string _BankNo = "";
        private string _AccNo = "";
        private int _AccType = 0;
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
 
        public string BankNo
        {
            get
            {
                return _BankNo;
            }
            set
            {
                _BankNo = value;
            }
        }
        public string AccNo
        {
            get
            {
                return _AccNo;
            }
            set
            {
                _AccNo = value;
            }
        }
        public int AccType
        {
            get
            {
                return _AccType;
            }
            set
            {
                _AccType = value;
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
