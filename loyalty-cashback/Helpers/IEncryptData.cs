using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Helpers
{
    public interface IEncryptData
    {
        public string EncryptDataFunction(string publicKey, object dataEncrypt);
        public object DecryptDataFunction(string publicKey, string dataDecrypt); 
    }
}
