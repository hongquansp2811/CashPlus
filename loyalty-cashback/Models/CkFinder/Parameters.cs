using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.Models
{
    public class Parameters
    {
        [System.ComponentModel.DefaultValue("")]
        public string command { get; set; }

        [System.ComponentModel.DefaultValue("vi")]
        public string lang { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string type { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string currentFolder { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string hash { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string newFolderName { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string fileName { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string newFileName { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string size { get; set; }
    }
}
