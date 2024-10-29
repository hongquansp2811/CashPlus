using LOYALTY.Data;
using LOYALTY.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LOYALTY.Controllers
{
    public class FileDetail
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class VerifyImageRequest
    {
        public string img_front { get; set; }
        public string? img_face { get; set; }
    }

    [Route("api/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private IConfiguration _config { get; set; }
        private LOYALTYContext _context { get; set; }
        private static readonly HttpClient client = new HttpClient();
        private static readonly System.IO.Stream MyStream;
        public UploadController(IConfiguration configuration, LOYALTYContext context)
        {
            _config = configuration;
            _context = context;
        }

        //[Authorize(Policy = "WebAdminUser")]
        [Route("uploadfile")]
        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files != null && files.Count() > 0)
            {
                long size = files.Sum(f => f.Length);
                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".svg", ".mp4", ".xlsx", ".xls", ".docx", ".doc", ".pdf", ".rar" };



                List<FileDetail> filePaths = new List<FileDetail>();
                int i = 0;
                string url = _config.GetSection("config")["Url"];
                string uploadFolder = "";
                uploadFolder = _config.GetSection("config")["Directory"] + "\\loyalty";
                foreach (var formFile in files)
                {
                    i += 1;
                    if (formFile.Length > 0)
                    {
                        try
                        {
                            //name folder
                            if (!Directory.Exists(uploadFolder))
                            {
                                Directory.CreateDirectory(uploadFolder);
                            }
                            var ext = formFile.FileName.Substring(formFile.FileName.LastIndexOf('.'));
                            var name = formFile.FileName.Substring(0, formFile.FileName.LastIndexOf('.'));
                            var extension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(extension))
                            {
                                var message = string.Format("Bạn vui lòng upload hình ảnh có định dạng: .jpg, .jpeg, .gif, .png, .bmp, .svg, .mp4, .xlsx, .xls, .docx, .doc, .pdf, .rar");
                                return new JsonResult(new { status = 400, code = "ERROR", data = message });
                            }
                            string file_name = Strings.RemoveDiacriticUrls(extension).ToUpper();
                            //name file
                            var uniqueFileName = Path.GetFileNameWithoutExtension(file_name).Replace(" ", "") + "_" + DateTime.Now.ToString("ssmmhhddMMyyyy") + i.ToString() + Path.GetExtension(formFile.FileName);
                            //path
                            var filePath = Path.Combine(uploadFolder, uniqueFileName);
                            if (!String.IsNullOrEmpty("dcvf"))
                            {
                                var x = new FileDetail();
                                x.name = uniqueFileName;
                                x.url = url + "loyalty" + "/" + uniqueFileName;
                                filePaths.Add(x);
                            }
                            else
                            {
                                var x = new FileDetail();
                                x.name = uniqueFileName;
                                x.url = url + uniqueFileName;
                                filePaths.Add(x);
                            }

                            using (var stream = System.IO.File.Create(filePath))
                            {
                                await formFile.CopyToAsync(stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult(new { status = 400, code = "ERROR", data = ex.Message });
                        }

                    }
                }
                return new JsonResult(new { status = 200, code = "OK", data = filePaths });
            }
            else
            {
                return new JsonResult(new { status = 400, code = "EMPTY_FILE" });
            }

        }

        //[Authorize(Policy = "AppUser")]
        [Route("uploadfile_app")]
        [HttpPost]
        public async Task<IActionResult> uploadfile_app(List<IFormFile> files)
        {
            if (files != null && files.Count() > 0)
            {
                long size = files.Sum(f => f.Length);
                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".svg", ".mp4", ".xlsx", ".xls", ".docx", ".doc", ".pdf", ".rar" };

                List<FileDetail> filePaths = new List<FileDetail>();
                int i = 0;
                string url = _config.GetSection("config")["Url"];
                string uploadFolder = "";
                uploadFolder = _config.GetSection("config")["Directory"] + "\\loyalty";
                foreach (var formFile in files)
                {
                    i += 1;
                    if (formFile.Length > 0)
                    {
                        try
                        {
                            //name folder
                            if (!Directory.Exists(uploadFolder))
                            {
                                Directory.CreateDirectory(uploadFolder);
                            }
                            var ext = formFile.FileName.Substring(formFile.FileName.LastIndexOf('.'));
                            var name = formFile.FileName.Substring(0, formFile.FileName.LastIndexOf('.'));
                            var extension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(extension))
                            {
                                var message = string.Format("Bạn vui lòng upload hình ảnh có định dạng: .jpg, .jpeg, .gif, .png, .bmp, .svg, .mp4, .xlsx, .xls, .docx, .doc, .pdf, .rar");
                                return new JsonResult(new { status = 400, code = "ERROR", data = message });
                            }
                            string file_name = Strings.RemoveDiacriticUrls(extension).ToUpper();
                            //name file
                            var uniqueFileName = Path.GetFileNameWithoutExtension(file_name).Replace(" ", "") + "_" + DateTime.Now.ToString("ssmmhhddMMyyyy") + i.ToString() + Path.GetExtension(formFile.FileName);
                            //path
                            var filePath = Path.Combine(uploadFolder, uniqueFileName);
                            if (!String.IsNullOrEmpty("dcvf"))
                            {
                                var x = new FileDetail();
                                x.name = uniqueFileName;
                                x.url = url + "loyalty" + "/" + uniqueFileName;
                                filePaths.Add(x);
                            }
                            else
                            {
                                var x = new FileDetail();
                                x.name = uniqueFileName;
                                x.url = url + uniqueFileName;
                                filePaths.Add(x);
                            }

                            using (var stream = System.IO.File.Create(filePath))
                            {
                                await formFile.CopyToAsync(stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult(new { status = 400, code = "ERROR", data = ex.Message });
                        }

                    }
                }
                return new JsonResult(new { status = 200, code = "OK", data = filePaths });
            }
            else
            {
                return new JsonResult(new { status = 400, code = "EMPTY_FILE" });
            }

        }

        //[Authorize(Policy = "WebPartnerUser")]
        [Route("uploadfile_store")]
        [HttpPost]
        public async Task<IActionResult> uploadfile_store(List<IFormFile> files)
        {
            if (files != null && files.Count() > 0)
            {
                long size = files.Sum(f => f.Length);
                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".svg", ".mp4", ".xlsx", ".xls", ".docx", ".doc", ".pdf", ".rar" };



                List<FileDetail> filePaths = new List<FileDetail>();
                int i = 0;
                string url = _config.GetSection("config")["Url"];
                string uploadFolder = "";
                uploadFolder = _config.GetSection("config")["Directory"] + "\\loyalty";
                foreach (var formFile in files)
                {
                    i += 1;
                    if (formFile.Length > 0)
                    {
                        try
                        {
                            //name folder
                            if (!Directory.Exists(uploadFolder))
                            {
                                Directory.CreateDirectory(uploadFolder);
                            }
                            var ext = formFile.FileName.Substring(formFile.FileName.LastIndexOf('.'));
                            var name = formFile.FileName.Substring(0, formFile.FileName.LastIndexOf('.'));
                            var extension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(extension))
                            {
                                var message = string.Format("Bạn vui lòng upload hình ảnh có định dạng: .jpg, .jpeg, .gif, .png, .bmp, .svg, .mp4, .xlsx, .xls, .docx, .doc, .pdf, .rar");
                                return new JsonResult(new { status = 400, code = "ERROR", data = message });
                            }
                            string file_name = Strings.RemoveDiacriticUrls(extension).ToUpper();
                            //name file
                            var uniqueFileName = Path.GetFileNameWithoutExtension(file_name).Replace(" ", "") + "_" + DateTime.Now.ToString("ssmmhhddMMyyyy") + i.ToString() + Path.GetExtension(formFile.FileName);
                            //path
                            var filePath = Path.Combine(uploadFolder, uniqueFileName);
                            if (!String.IsNullOrEmpty("dcvf"))
                            {
                                var x = new FileDetail();
                                x.name = uniqueFileName;
                                x.url = url + "loyalty" + "/" + uniqueFileName;
                                filePaths.Add(x);
                            }
                            else
                            {
                                var x = new FileDetail();
                                x.name = uniqueFileName;
                                x.url = url + uniqueFileName;
                                filePaths.Add(x);
                            }

                            using (var stream = System.IO.File.Create(filePath))
                            {
                                await formFile.CopyToAsync(stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult(new { status = 400, code = "ERROR", data = ex.Message });
                        }

                    }
                }
                return new JsonResult(new { status = 200, code = "OK", data = filePaths });
            }
            else
            {
                return new JsonResult(new { status = 400, code = "EMPTY_FILE" });
            }

        }
    }
}
