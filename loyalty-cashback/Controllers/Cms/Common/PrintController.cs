using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using LOYALTY.Helpers;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using LOYALTY.Data;
using LOYALTY.Models;
using LOYALTY.DataObjects.Response;
using Microsoft.AspNetCore.Authorization;

namespace LOYALTY.Controllers
{
    [Route("api/print")]
    [ApiController]
    public class PrintController : ControllerBase
    {
        public class PrintRequestResponse
        {
            public string? PartnerName { get; set; }
        }

        private IConfiguration _config { get; set; }
        private readonly LOYALTYContext _context;
        private readonly ICommonFunction _commonFunction;
        public PrintController(IConfiguration configuration, LOYALTYContext context, ICommonFunction commonFunction)
        {
            _config = configuration;
            _context = context;
            _commonFunction = commonFunction;
        }

        [Authorize(Policy = "WebAdminUser")]
        [Route("printInviteFile/{partner_id}")]
        [HttpGet]
        public async Task<ActionResult> PrintInviteFile(Guid partner_id)
        {
            var requestObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (requestObj == null)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            string url = _config.GetSection("config")["Url"];
            string uploadFolder = "";
            uploadFolder = _config.GetSection("config")["Directory"] + "\\loyalty";
            string uniqueFileName = "Thungohoptackinhdoanh_" + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".docx";
         
            string contentType = "application/vnd.ms-word.document.12";

            FormatType type = FormatType.Docx;
            //path
            var filePath = Path.Combine(uploadFolder, uniqueFileName);
            //name folder
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            WordDocument doc = new WordDocument();

            try
            {
                // Create a new document
                // Load the template.
                string dataPath = "./TemplatePrint/THU_NGO.docx";
                FileStream fileStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                doc.Open(fileStream, FormatType.Automatic);

                //Create MailMergeDataTable
                MailMergeDataTable mailMergeDataTableOrder = GetRequest(requestObj);

                // Execute Mail Merge with groups.
                doc.MailMerge.ExecuteGroup(mailMergeDataTableOrder);
                // Using Merge events to do conditional formatting during runtime.
                doc.MailMerge.MergeField += new MergeFieldEventHandler(MailMerge_MergeField);

            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
            FileStream outputStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            doc.Save(outputStream, type);
            doc.Close();
            outputStream.Flush();
            outputStream.Dispose();

            return new JsonResult(new APIResponse(new
            {
                filePath = url + "loyalty/"
             + uniqueFileName
            }))
            { StatusCode = 200 };
        }
        
        [Authorize(Policy = "WebPartnerUser")]
        [Route("printInviteFile_store/{partner_id}")]
        [HttpGet]
        public async Task<ActionResult> printInviteFile_store(Guid partner_id)
        {
            var requestObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (requestObj == null)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            string url = _config.GetSection("config")["Url"];
            string uploadFolder = "";
            uploadFolder = _config.GetSection("config")["Directory"] + "\\loyalty";
            string uniqueFileName = "Thungohoptackinhdoanh_" + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".docx";
         
            string contentType = "application/vnd.ms-word.document.12";

            FormatType type = FormatType.Docx;
            //path
            var filePath = Path.Combine(uploadFolder, uniqueFileName);
            //name folder
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            WordDocument doc = new WordDocument();

            try
            {
                // Create a new document
                // Load the template.
                string dataPath = "./TemplatePrint/THU_NGO.docx";
                FileStream fileStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                doc.Open(fileStream, FormatType.Automatic);

                //Create MailMergeDataTable
                MailMergeDataTable mailMergeDataTableOrder = GetRequest(requestObj);

                // Execute Mail Merge with groups.
                doc.MailMerge.ExecuteGroup(mailMergeDataTableOrder);
                // Using Merge events to do conditional formatting during runtime.
                doc.MailMerge.MergeField += new MergeFieldEventHandler(MailMerge_MergeField);

            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
            FileStream outputStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            doc.Save(outputStream, type);
            doc.Close();
            outputStream.Flush();
            outputStream.Dispose();

            return new JsonResult(new APIResponse(new
            {
                filePath = url + "loyalty/"
             + uniqueFileName
            }))
            { StatusCode = 200 };
        }

        [Authorize(Policy = "AppUser")]
        [Route("printInviteFile_app/{partner_id}")]
        [HttpGet]
        public async Task<ActionResult> printInviteFile_app(Guid partner_id)
        {
            var requestObj = _context.Partners.Where(x => x.id == partner_id).FirstOrDefault();

            if (requestObj == null)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }

            string url = _config.GetSection("config")["Url"];
            string uploadFolder = "";
            uploadFolder = _config.GetSection("config")["Directory"] + "\\loyalty";
            string uniqueFileName = "Thungohoptackinhdoanh_" + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".docx";
         
            string contentType = "application/vnd.ms-word.document.12";

            FormatType type = FormatType.Docx;
            //path
            var filePath = Path.Combine(uploadFolder, uniqueFileName);
            //name folder
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            WordDocument doc = new WordDocument();

            try
            {
                // Create a new document
                // Load the template.
                string dataPath = "./TemplatePrint/THU_NGO.docx";
                FileStream fileStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                doc.Open(fileStream, FormatType.Automatic);

                //Create MailMergeDataTable
                MailMergeDataTable mailMergeDataTableOrder = GetRequest(requestObj);

                // Execute Mail Merge with groups.
                doc.MailMerge.ExecuteGroup(mailMergeDataTableOrder);
                // Using Merge events to do conditional formatting during runtime.
                doc.MailMerge.MergeField += new MergeFieldEventHandler(MailMerge_MergeField);

            }
            catch (Exception ex)
            {
                return new JsonResult(new APIResponse(400)) { StatusCode = 200 };
            }
            FileStream outputStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            doc.Save(outputStream, type);
            doc.Close();
            outputStream.Flush();
            outputStream.Dispose();

            return new JsonResult(new APIResponse(new
            {
                filePath = url + "loyalty/"
             + uniqueFileName
            }))
            { StatusCode = 200 };
        }
        private void MailMerge_MergeField(object sender, MergeFieldEventArgs args)
        {
        }

        private MailMergeDataTable GetRequest(Partner requestData)
        {

            List<PrintRequestResponse> listRequests = new List<PrintRequestResponse>();
            PrintRequestResponse newResponse = new PrintRequestResponse();
            newResponse.PartnerName = requestData.name;
            listRequests.Add(newResponse);
            MailMergeDataTable dataTable = new MailMergeDataTable("PartnerInfo", listRequests);
            return dataTable;
        }
    }
}
