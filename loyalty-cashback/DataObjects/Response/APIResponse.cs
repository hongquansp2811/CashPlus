using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.DataObjects.Response
{
    public class APIResponse
    {
        public string code { get; set; }
        public string error { get; set; }
        public object data { get; set; }

        /// <summary>
        /// Return Error Status Code & Error Code.
        /// </summary>
        public APIResponse(int status)
        {
            switch (status)
            {
                case 200:
                    this.code = "200";
                    break;
                case 400:
                    this.error = "BAD_REQUEST";
                    this.code = "400";
                    break;
                case 404:
                    this.error = "NO_DATA_FOUND";
                    this.code = "404";
                    break;
                default:
                    this.code = "400";
                    break;
            }
        }

        public APIResponse(object data)
        {
            this.code = "200";
            this.data = data;
            this.error = null;
        }

        public APIResponse(int status, string data)
        {
            this.code = status.ToString();
            this.data = data;
            this.error = null;
        }

        public APIResponse(int status, object data)
        {
            this.code = status.ToString();
            this.data = data;
            this.error = null;
        }

        public APIResponse(string error)
        {
            this.code = "400";
            this.error = error;
            this.data = null;
        }
    }
}
