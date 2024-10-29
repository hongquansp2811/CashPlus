using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Request
{
    public class DecryptRequest
    {
        public string? secret_key { get; set; }
        public string? dataDecrypt { get; set; }
    }
}
