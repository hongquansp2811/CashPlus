﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class DeleteFolder
    {
        public string resourceType { get; set; }
        public CurrentFolder currentFolder { get; set; }
        public int deleted { get; set; }
    }
}
