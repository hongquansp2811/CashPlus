using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class FileUploadData
    {
        public Files upload { get; set; }
        public string ckCsrfToken { get; set; }
    }
}
