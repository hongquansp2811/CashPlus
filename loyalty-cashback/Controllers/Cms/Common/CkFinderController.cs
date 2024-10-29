using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LOYALTY.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace LOYALTY.Controllers
{
    [Route("ckfinder/connector")]
    [ApiController]
    public class CkFinderController : Controller
    {
        private IConfiguration _configuration { get; }

        public CkFinderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Get(string key)
        {
            return Request.Cookies[key];
        }
        [HttpGet]
        public async Task<ActionResult> Connector([FromQuery] Parameters paging)
        {
            var jwtToken = Get("accessToken");
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            //if (string.IsNullOrEmpty(jwtToken))
            //{
            //    //var vMessage = "Lỗi đăng nhâp:";

            //    //return Ok(new { error = vMessage });

            //    var vMessage = "Chức năng này cần phải đăng nhập vào hệ thống";
            //    return Content(vMessage, "text/html");

            //}

            //var tokenValidationParameters = new TokenValidationParameters
            //{
            //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["AppSettings:JwtKey"])),
            //    ValidateIssuer = false, //_options.Value.TokenValidationParameter.ValidateIssuer,
            //    ValidateAudience = false,// _options.Value.TokenValidationParameter.ValidateAudience, //you might want to validate the audience and issuer depending on your use case
            //    ValidateLifetime = true,// _options.Value.TokenValidationParameter.ValidateLifetime, //here we are saying that we don't care about the token's expiration date
            //    ValidateIssuerSigningKey = true,// _options.Value.TokenValidationParameter.ValidateIssuerSigningKey,
            //    ValidIssuer = "https://localhost:5100/",
            //    ClockSkew = TimeSpan.Zero
            //};

            //try
            //{
            //    var tokenHandler = new JwtSecurityTokenHandler();
            //    var principal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out SecurityToken securityToken);
            //    var jwtSecurityToken = securityToken as JwtSecurityToken;

            //    if (jwtSecurityToken == null)
            //    {

            //        var vMessage = "Chức năng này cần phải đăng nhập vào hệ thống";
            //        return Content(vMessage, "text/html");


            //    }
            //    else if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        var vMessage = "Chức năng này cần phải đăng nhập vào hệ thống";
            //        return Content(vMessage, "text/html");
            //    }
            //}
            //catch (Exception e)
            //{

            //    var vMessage = "Chức năng này cần phải đăng nhập vào hệ thống, " + e.ToString();
            //    return Content(vMessage, "text/html");
            //}

            //if (paging.currentFolder.Contains("../") || paging.currentFolder.IndexOfAny(Path.GetInvalidPathChars()) > -1
            //|| paging.fileName?.IndexOfAny(Path.GetInvalidFileNameChars()) > -1
            //)
            //{
            //    throw new Exception("Tham số không hợp lệ!");
            //}

            string rootUploads = _configuration.GetSection("config")["rootUploads"];
            string webRootPath = _configuration.GetSection("config")["Directory"];
            string folderName = rootUploads + "/" + paging.type + paging.currentFolder;
            string path = Path.Combine(webRootPath, folderName);

            //var identity = (ClaimsIdentity)User.Identity;
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if(access_key=="" || access_key==null)
            //    return Ok(new NotFoundResult());
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)Const.Action.VIEW))
            //{
            //    return Ok(new NotFoundResult());
            //}
            if (paging.command == "Init")
            {
                Init init = new Init();
                init.s = "";
                init.c = "";
                init.enabled = true;
                init.uploadMaxSize = 8388608;
                init.uploadCheckImages = false;
                List<string> thumbs = new List<string>();
                thumbs.Add("150x150");
                thumbs.Add("300x300");
                thumbs.Add("500x500");
                init.thumbs = thumbs;

                Images images = new Images();
                images.max = "1600x1200";

                Sizes sizes = new Sizes();
                sizes.small = "480x320";
                sizes.medium = "600x480";
                sizes.large = "800x600";
                images.sizes = sizes;

                init.images = images;

                List<ResourceTypes> resourceTypes = new List<ResourceTypes>();
                ResourceTypes resourceType = new ResourceTypes();
                resourceType.acl = 1023;
                resourceType.allowedExtensions = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,jfif";
                resourceType.deniedExtensions = "";
                resourceType.hasChildren = true;
                resourceType.hash = "c0df46105183aec6";
                resourceType.maxSize = 8388608;
                resourceType.name = "Files";
                resourceType.url = "/files/";
                resourceTypes.Add(resourceType);

                ResourceTypes resourceType2 = new ResourceTypes();
                resourceType2.acl = 1023;
                resourceType2.allowedExtensions = "bmp,gif,jpeg,jpg,png";
                resourceType2.deniedExtensions = "";
                resourceType2.hasChildren = true;
                resourceType2.hash = "45a5762642ef885a";
                resourceType2.maxSize = 8388608;
                resourceType2.name = "Images";
                resourceType2.url = "/images/";
                resourceTypes.Add(resourceType2);

                init.resourceTypes = resourceTypes;
                return Ok(init);
            }
            else if (paging.command == "GetFolders")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                GetFolders getFolders = new GetFolders();
                getFolders.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                currentFolder.acl = 1023;
                getFolders.currentFolder = currentFolder;

                //Lấy danh sách folder
                var directories = Directory.GetDirectories(path);

                List<Folders> folders = new List<Folders>();
                foreach (var item in directories)
                {
                    if (!item.Split('/').LastOrDefault().StartsWith("_thumb"))
                    {
                        Folders folder = new Folders();
                        folder.name = item.Split('/').LastOrDefault();

                        var directoriesChild = Directory.GetDirectories(item);

                        folder.hasChildren = directoriesChild.Count() > 0 ? true : false;
                        folder.acl = 1023;
                        folders.Add(folder);
                    }
                }
                getFolders.folders = folders;
                return Ok(getFolders);
            }
            else if (paging.command == "GetFiles")
            {

                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }

                GetFiles getFiles = new GetFiles();
                getFiles.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + currentFolder.path;
                currentFolder.acl = 1023;
                getFiles.currentFolder = currentFolder;



                //Lấy đường dẫn folder
                //if (!Directory.Exists(path))
                //{
                //    Directory.CreateDirectory(path);
                //}
                //var directories = Directory.GetDirectories(path);
                DirectoryInfo d = new DirectoryInfo(path);
                FileInfo[] Files = d.GetFiles(); //Getting Text files

                List<Files> files = new List<Files>();
                foreach (FileInfo item in Files)
                {
                    Files file = new Files();
                    file.name = item.Name;
                    string date = "";
                    date += item.CreationTime.Year;
                    date += item.CreationTime.Month < 10 ? ("0" + item.CreationTime.Month) : item.CreationTime.Month + "";
                    date += item.CreationTime.Day < 10 ? ("0" + item.CreationTime.Day) : item.CreationTime.Day + "";
                    date += item.CreationTime.Hour < 10 ? ("0" + item.CreationTime.Hour) : item.CreationTime.Hour + "";
                    date += item.CreationTime.Minute < 10 ? ("0" + item.CreationTime.Minute) : item.CreationTime.Minute + "";
                    date += item.CreationTime.Second < 10 ? ("0" + item.CreationTime.Second) : item.CreationTime.Second + "";
                    file.date = date;
                    file.size = (int)item.Length / 1024;
                    files.Add(file);
                }

                getFiles.files = files;
                return Ok(getFiles);
            }
            else if (paging.command == "Thumbnail")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                string folderThumb = rootUploads + "/_thumbs/" + paging.type + paging.currentFolder;
                string pathThumb = Path.Combine(path);
                var file = await Thumbnail(paging, pathThumb);
                return file;
            }
            else if (paging.command == "GetResizedImages")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }

                GetResizedImages getResizedImages = new GetResizedImages();
                getResizedImages.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + currentFolder.path;
                currentFolder.acl = 1023;
                getResizedImages.currentFolder = currentFolder;
                getResizedImages.originalSize = "1920x1200";
                string imageName = paging.fileName.Substring(0, paging.fileName.LastIndexOf('.'));
                string imageType = paging.fileName.Substring(paging.fileName.LastIndexOf('.'));
                Resized resized = new Resized();
                resized.small = imageName + "__480x300" + imageType;
                resized.medium = imageName + "__600x375" + imageType;
                resized.large = imageName + "__800x500" + imageType;
                List<string> custom = new List<string>();
                custom.Add(imageName + "__200x125" + imageType);
                custom.Add(imageName + "__300x188" + imageType);
                resized.__custom = custom;
                getResizedImages.resized = resized;
                return Ok(getResizedImages);
            }
            else if (paging.command == "DownloadFile")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }

                if (ConvertUrlpath(paging.fileName).Contains("../") || ConvertUrlpath(paging.fileName).Contains("..\\") || ConvertUrlpath(paging.fileName).IndexOfAny(Path.GetInvalidPathChars()) > -1) if (ConvertUrlpath(paging.fileName).Contains("../") || ConvertUrlpath(paging.fileName).Contains("..\\") || ConvertUrlpath(paging.fileName).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                    {
                        var vMessage = "Tên file không hợp lệ!";
                        return Content(vMessage, "text/html");
                    }

                var ext = Path.GetExtension(paging.fileName).TrimStart('.');
                var allow = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,bmp,gif,jpeg,jpg,png,jfif";
                if (!allow.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(ext))
                {

                    var vMessage = "Tên file không hợp lệ!";
                    return Content(vMessage, "text/html"); ;

                }



                var file = await Thumbnail(paging, path);
                return file;
            }
            else if (paging.command == "GetFileUrl")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                GetFileUrl getFileUrl = new GetFileUrl();
                getFileUrl.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + currentFolder.path;
                currentFolder.acl = 1023;
                getFileUrl.currentFolder = currentFolder;
                getFileUrl.url = currentFolder.url + paging.fileName;

                return Ok(getFileUrl);
            }
            else if (paging.command == "ImageInfo")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                ImageInfo imageInfo = new ImageInfo();
                imageInfo.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + currentFolder.path;
                currentFolder.acl = 1023;
                imageInfo.currentFolder = currentFolder;

                string fileName = path + paging.fileName;
                System.Drawing.Image img = Image.FromFile(fileName);
                ImageFormat format = img.RawFormat;

                imageInfo.width = img.Width;
                imageInfo.height = img.Height;

                return Ok(imageInfo);
            }
            else if (paging.command == "ImagePreview")
            {
                var file = await Thumbnail(paging, path);
                return file;
            }
            return Ok();
        }
        public static string ConvertUrlpath(string text)
        {
            string res = "";
            if (text != null)
            {
                res = text;
                res = res.Replace("%2e%2e%2f", "../");
                res = res.Replace("%2e%2e", "../");
                res = res.Replace("..%2f", "../");
                res = res.Replace("%2e%2e%5c", "..\\");
                res = res.Replace("%2e%2e\\", "..\\");
                res = res.Replace("..%5c", "..\\");
                res = res.Replace("%252e%252e%255c", "..\\");
                res = res.Replace("..%255c", "..\\");

            }
            return res;
        }
        [HttpGet("{path}")]
        public async Task<FileResult> Thumbnail([FromQuery] Parameters paging, [FromRoute] string path)
        {
            string filePath = path + "/" + paging.fileName;

            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    await file.ReadAsync(bytes, 0, (int)file.Length);
                    await ms.WriteAsync(bytes, 0, (int)file.Length);
                    return File(bytes, System.Net.Mime.MediaTypeNames.Image.Jpeg, paging.fileName);
                }
            }
        }
        public static void createThumb(int thumbWidth, Image image, string thumbPath, string file_type)
        {
            double srcWidth = image.Width;
            double srcHeight = image.Height;
            if (thumbWidth > srcWidth) thumbWidth = (int)srcWidth;
            double thumbHeight = (srcHeight / srcWidth) * thumbWidth;
            if (thumbHeight > srcHeight) thumbHeight = (int)srcHeight;
            Bitmap bmp = new Bitmap(thumbWidth, (int)thumbHeight);

            Graphics gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.CompositingQuality = CompositingQuality.HighQuality;
            gr.InterpolationMode = InterpolationMode.High;

            Rectangle rectDestination = new Rectangle(0, 0, thumbWidth, (int)thumbHeight);
            gr.DrawImage(image, rectDestination, 0, 0, (int)srcWidth, (int)srcHeight, GraphicsUnit.Pixel);

            if (file_type.ToLower() == ".jpg" || file_type.ToLower() == ".jpeg")
                bmp.Save(thumbPath, ImageFormat.Jpeg);
            else if (file_type.ToLower() == ".png")
                bmp.Save(thumbPath, ImageFormat.Png);
            else if (file_type.ToLower() == ".gif")
                bmp.Save(thumbPath, ImageFormat.Gif);
            else
                bmp.Save(thumbPath, ImageFormat.Jpeg);

            bmp.Dispose();
            image.Dispose();
        }
        [HttpPost]
        public ActionResult ConnectorPost([FromQuery] Parameters paging)
        {

            string rootUploads = _configuration.GetSection("config")["rootUploads"];
            string webRootPath = _configuration.GetSection("config")["Directory"];
            //string folderName = rootUploads + "/" + paging.type + paging.currentFolder;
            //string path = Path.Combine(webRootPath, folderName);

            if (paging.command == "FileUpload")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {

                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }

                var httpRequest = Request.Form.Files;
                foreach (var file in httpRequest)
                {

                    string folderName = rootUploads + "/" + paging.type + paging.currentFolder;
                    string rel = rootUploads + "/_thumbs/" + paging.type + paging.currentFolder + file.FileName;
                    string fullPath = Path.Combine(webRootPath, folderName);
                    fullPath += file.FileName;

                    if (file.Length > 0)
                    {

                        if (ConvertUrlpath(file.FileName).Contains("../") || ConvertUrlpath(file.FileName).Contains("..\\") || ConvertUrlpath(file.FileName).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                        {
                            var vMessage = "Tên file không hợp lệ!";
                            return Content(vMessage, "text/html");
                        }
                        var ext = Path.GetExtension(file.FileName).TrimStart('.');
                        var allow = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,bmp,gif,jpeg,jpg,jfif,webp,PNG";
                        if (!allow.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(ext))
                        {

                            var vMessage = "Tên file không hợp lệ!";
                            return Content(vMessage, "text/html"); ;

                        }
                        try
                        {
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                                var image = Bitmap.FromStream(stream);
                                //resize image
                                string typeFile = "." + file.FileName.Split('.').LastOrDefault();
                                //int size = int.Parse(paging.size.Split('x').FirstOrDefault());
                                //createThumb(920, image, fullPath, typeFile);

                                string thumbPath = Path.Combine(webRootPath, rel);
                                createThumb(108, image, thumbPath, typeFile);

                                FileUpload fileUpload = new FileUpload();
                                fileUpload.resourceType = paging.type;
                                CurrentFolder currentFolder = new CurrentFolder();
                                currentFolder.path = paging.currentFolder;
                                //if (paging.currentFolder == null)
                                //    paging.currentFolder = paging.currentFolder + "/" + DateTime.Now.Year + "/" + DateTime.Now.Month;
                                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                                currentFolder.acl = 1023;
                                fileUpload.fileName = file.FileName;
                                fileUpload.uploaded = 1;

                                return Ok(fileUpload);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            else if (paging.command == "CopyFiles")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                string data = Request.Form["jsonData"].ToString();
                JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(data);
                string targetPath = webRootPath + rootUploads + "/" + paging.type + paging.currentFolder;
                string targetPathThumb = webRootPath + rootUploads + "/_thumbs/" + paging.type + paging.currentFolder;




                foreach (var item in jsonData.files)
                {


                    string sourcePath = webRootPath + rootUploads + "/" + item.type + item.folder;
                    string sourceFile = Path.Combine(sourcePath, item.name);
                    string destFile = Path.Combine(targetPath, item.name);


                    if (ConvertUrlpath(destFile).Contains("../") || ConvertUrlpath(destFile).Contains("..\\") || ConvertUrlpath(destFile).IndexOfAny(Path.GetInvalidPathChars()) > -1 || ConvertUrlpath(sourceFile).Contains("../") || ConvertUrlpath(sourceFile).Contains("..\\") || sourceFile.IndexOfAny(Path.GetInvalidPathChars()) > -1)
                    {
                        var vMessage = "Tên file không hợp lệ!";
                        return Content(vMessage, "text/html");
                    }

                    var ext = Path.GetExtension(destFile).TrimStart('.');
                    var allow = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,bmp,gif,jpeg,jpg,png,jfif";
                    if (!allow.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(ext))
                    {

                        var vMessage = "Tên file không hợp lệ!";
                        return Content(vMessage, "text/html"); ;

                    }

                    System.IO.File.Copy(sourceFile, destFile, false);

                    //copy file thumb
                    string sourcePathThumb = webRootPath + rootUploads + "/_thumbs/" + item.type + item.folder;
                    string sourceFileThumb = Path.Combine(sourcePathThumb, item.name);
                    string destFileThumb = Path.Combine(targetPathThumb, item.name);
                    System.IO.File.Copy(sourceFileThumb, destFileThumb, false);
                }

                CopyFiles copyFiles = new CopyFiles();
                copyFiles.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                currentFolder.acl = 1023;
                copyFiles.currentFolder = currentFolder;
                copyFiles.copied = jsonData.files.Count();

                return Ok(copyFiles);
            }
            else if (paging.command == "CreateFolder")
            {
                //create folder
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                string folderName = rootUploads + "/" + paging.type + paging.currentFolder + paging.newFolderName;
                string path = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //create folder thumbs
                string folderChildName = rootUploads + "/_thumbs/" + paging.type + paging.currentFolder + paging.newFolderName;
                string pathThumb = Path.Combine(webRootPath, folderChildName);
                if (!Directory.Exists(pathThumb))
                {
                    Directory.CreateDirectory(pathThumb);
                }

                CreateFolder createFolder = new CreateFolder();
                createFolder.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                currentFolder.acl = 1023;
                createFolder.currentFolder = currentFolder;
                createFolder.newFolder = paging.newFolderName;
                createFolder.created = 1;

                return Ok(createFolder);
            }
            else if (paging.command == "DeleteFiles")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                if (ConvertUrlpath(paging.fileName).Contains("../") || ConvertUrlpath(paging.fileName).Contains("..\\") || ConvertUrlpath(paging.fileName).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Tên file không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                string folderName = rootUploads + "/" + paging.type + paging.currentFolder + paging.newFolderName;
                string path = Path.Combine(webRootPath, folderName);

                string oldName = path + paging.fileName;
                string newName = path + paging.newFileName;

                var ext = Path.GetExtension(newName).TrimStart('.');
                var allow = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,bmp,gif,jpeg,jpg,png,jfif";
                if (!allow.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(ext))
                {

                    var vMessage = "Tên file không hợp lệ!";
                    return Content(vMessage, "text/html"); ;

                }
                //delete file


                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }


                DeleteFolder deleteFolder = new DeleteFolder();
                deleteFolder.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                currentFolder.acl = 1023;
                deleteFolder.currentFolder = currentFolder;
                deleteFolder.deleted = 2;
                return Ok(deleteFolder);
            }
            else if (paging.command == "DeleteFolder")
            {
                //delete folder
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                string folderName = rootUploads + "/" + paging.type + paging.currentFolder;
                string path = Path.Combine(webRootPath, folderName);
                if (Directory.Exists(path))
                {
                    //delete file
                    DirectoryInfo d = new DirectoryInfo(path);
                    FileInfo[] Files = d.GetFiles(); //Getting Text files
                    foreach (FileInfo item in Files)
                    {
                        System.IO.File.Delete(item.FullName);
                    }

                    Directory.Delete(path);
                }

                //delete folder thumbs
                string folderChildName = rootUploads + "/_thumbs/" + paging.type + paging.currentFolder + paging.newFolderName;
                string pathThumb = Path.Combine(webRootPath, folderChildName);
                if (Directory.Exists(pathThumb))
                {
                    //delete file
                    DirectoryInfo d = new DirectoryInfo(pathThumb);
                    FileInfo[] Files = d.GetFiles(); //Getting Text files
                    foreach (FileInfo item in Files)
                    {
                        System.IO.File.Delete(item.FullName);
                    }
                    Directory.Delete(pathThumb);
                }

                DeleteFolder deleteFolder = new DeleteFolder();
                deleteFolder.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                currentFolder.acl = 1023;
                deleteFolder.currentFolder = currentFolder;
                deleteFolder.deleted = 1;
                return Ok(deleteFolder);
            }
            else if (paging.command == "ImageEdit")
            {
                string data = Request.Form["jsonData"].ToString();
                JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(data);

                return Ok();
            }
            else if (paging.command == "ImageResize")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                string folderName = rootUploads + "/" + paging.type + paging.currentFolder + paging.fileName;

                string fullPath = Path.Combine(webRootPath, folderName);
                Image image = Image.FromFile(fullPath);
                //string typeFile = "." + paging.fileName.Split('.').LastOrDefault();
                string imageName = paging.fileName.Substring(0, paging.fileName.LastIndexOf('.'));
                string typeFile = paging.fileName.Substring(paging.fileName.LastIndexOf('.'));
                int size = int.Parse(paging.size.Split('x').FirstOrDefault());
                createThumb(size, image, fullPath, typeFile);

                int height = (size * image.Height) / image.Width;
                string fileName = imageName + "__" + size + "x" + height + typeFile;

                ImageResize imageResize = new ImageResize();
                imageResize.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                currentFolder.acl = 1023;
                imageResize.currentFolder = currentFolder;
                imageResize.url = "/" + folderName + "__thumbs/" + paging.fileName + "/" + fileName;
                return Ok(imageResize);
            }
            else if (paging.command == "MoveFiles")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }




                //      string oldName = path + paging.fileName;
                //       string newName = path + paging.newFileName;



                string data = Request.Form["jsonData"].ToString();
                JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(data);
                string targetPath = webRootPath + rootUploads + "/" + paging.type + paging.currentFolder;
                string targetPathThumb = webRootPath + rootUploads + "/_thumbs/" + paging.type + paging.currentFolder;
                foreach (var item in jsonData.files)
                {

                    var ext = Path.GetExtension(item.name).TrimStart('.');
                    var allow = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,bmp,gif,jpeg,jpg,png,jfif";
                    if (!allow.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(ext))
                    {

                        var vMessage = "Tên file không hợp lệ!";
                        return Content(vMessage, "text/html"); ;

                    }
                    string sourcePath = webRootPath + rootUploads + "/" + item.type + item.folder;
                    string sourceFile = Path.Combine(sourcePath, item.name);
                    string destFile = Path.Combine(targetPath, item.name);
                    System.IO.File.Move(sourceFile, destFile);

                    //move thumb
                    string sourcePathThumb = webRootPath + rootUploads + "/_thumbs/" + item.type + item.folder;
                    string sourceFileThumb = Path.Combine(sourcePathThumb, item.name);
                    string destFileThumb = Path.Combine(targetPathThumb, item.name);
                    System.IO.File.Move(sourceFileThumb, destFileThumb);
                }

                MoveFiles moveFiles = new MoveFiles();
                moveFiles.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + paging.currentFolder;
                currentFolder.acl = 1023;
                moveFiles.currentFolder = currentFolder;
                moveFiles.moved = jsonData.files.Count();

                return Ok(moveFiles);
            }
            else if (paging.command == "QuickUpload")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                if (ConvertUrlpath(paging.fileName).Contains("../") || ConvertUrlpath(paging.fileName).Contains("..\\") || ConvertUrlpath(paging.fileName).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Tên file không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                var httpRequest = Request.Form.Files;
                foreach (var file in httpRequest)
                {



                    string folderName = webRootPath + rootUploads + "/" + paging.type + paging.currentFolder;
                    string rel = webRootPath + rootUploads + "/_thumbs/" + paging.type + paging.currentFolder + file.FileName;
                    string fullPath = Path.Combine(webRootPath, folderName);
                    fullPath += file.FileName;


                    //      string oldName = path + paging.fileName;
                    //       string newName = path + paging.newFileName;

                    var ext = Path.GetExtension(file.FileName).TrimStart('.');
                    var allow = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,bmp,gif,jpeg,jpg,png,jfif";
                    if (!allow.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(ext))
                    {

                        var vMessage = "Tên file không hợp lệ!";
                        return Content(vMessage, "text/html"); ;

                    }

                    if (file.Length > 0)
                    {
                        try
                        {
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                                var image = Bitmap.FromStream(stream);
                                string thumbPath = Path.Combine(webRootPath, rel);
                                string typeFile = "." + file.FileName.Split('.').LastOrDefault();
                                createThumb(355, image, thumbPath, typeFile);

                                FileUpload fileUpload = new FileUpload();
                                fileUpload.resourceType = paging.type;
                                CurrentFolder currentFolder = new CurrentFolder();
                                currentFolder.path = paging.currentFolder;
                                currentFolder.url = "/" + rootUploads + paging.type + paging.currentFolder;
                                currentFolder.acl = 1023;
                                fileUpload.fileName = file.FileName;
                                fileUpload.uploaded = 1;
                                return Ok(fileUpload);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            else if (paging.command == "RenameFile")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }

                if (ConvertUrlpath(paging.fileName).Contains("../") || ConvertUrlpath(paging.fileName).Contains("..\\") || ConvertUrlpath(paging.fileName).IndexOfAny(Path.GetInvalidPathChars()) > -1 || ConvertUrlpath(paging.newFileName).Contains("../") || ConvertUrlpath(paging.newFileName).Contains("..\\") || ConvertUrlpath(paging.newFileName).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Tên file không hợp lệ!";
                    return Content(vMessage, "text/html");
                }

                //rename file
                string folderName = rootUploads + "/" + paging.type + paging.currentFolder;
                string path = Path.Combine(webRootPath, folderName);
                string oldName = path + paging.fileName;
                string newName = path + paging.newFileName;

                var ext = Path.GetExtension(newName).TrimStart('.');
                var allow = "7z,aiff,asf,avi,bmp,csv,doc,docx,fla,flv,gif,gz,gzip,jpeg,jpg,mid,mov,mp3,mp4,mpc,mpeg,mpg,ods,odt,pdf,png,ppt,pptx,pxd,qt,ram,rar,rm,rmi,rmvb,rtf,sdc,sitd,swf,sxc,sxw,tar,tgz,tif,tiff,txt,vsd,wav,wma,wmv,xls,xlsx,zip,bmp,gif,jpeg,jpg,png,jfif";
                if (!allow.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(ext))
                {

                    var vMessage = "Tên file không hợp lệ!";
                    return Content(vMessage, "text/html"); ;

                }
                System.IO.File.Move(oldName, newName);
                //rename file thumb
                string folderNameThumb = rootUploads + "/_thumbs/" + paging.type + paging.currentFolder;
                string pathThumb = Path.Combine(webRootPath, folderNameThumb);
                string oldNameThumb = pathThumb + paging.fileName;
                string newNameThumb = pathThumb + paging.newFileName;
                //System.IO.File.Move(oldNameThumb, newNameThumb);

                RenameFile renameFile = new RenameFile();
                renameFile.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + currentFolder.path;
                currentFolder.acl = 1023;
                renameFile.currentFolder = currentFolder;
                renameFile.name = paging.fileName;
                renameFile.newName = paging.newFileName;
                renameFile.renamed = 1;
                return Ok(renameFile);
            }
            else if (paging.command == "RenameFolder")
            {
                if (ConvertUrlpath(paging.currentFolder).Contains("../") || ConvertUrlpath(paging.currentFolder).Contains("..\\") || ConvertUrlpath(paging.currentFolder).IndexOfAny(Path.GetInvalidPathChars()) > -1 || ConvertUrlpath(paging.newFolderName).Contains("../") || ConvertUrlpath(paging.newFolderName).Contains("..\\") || ConvertUrlpath(paging.newFolderName).IndexOfAny(Path.GetInvalidPathChars()) > -1)
                {
                    var vMessage = "Thư mục không hợp lệ!";
                    return Content(vMessage, "text/html");
                }
                string folderName = rootUploads + "/" + paging.type;
                string folderNameThumb = rootUploads + "/_thumbs/" + paging.type;
                string path = Path.Combine(webRootPath, folderName);
                string pathThumb = Path.Combine(webRootPath, folderNameThumb);

                string[] folder = paging.currentFolder.Split('/');
                string oldName = path + paging.currentFolder;
                string oldNameThumb = pathThumb + paging.currentFolder;
                string newName = path;
                string newNameThumb = pathThumb;
                if (folder.Length >= 2)
                {
                    for (int i = 0; i < folder.Length - 2; i++)
                    {
                        newName += folder[i] + "/";
                        newNameThumb += folder[i] + "/";
                    }
                }
                newName += paging.newFolderName;
                newNameThumb += paging.newFolderName;
                Directory.Move(oldName, newName);
                //rename thumb
                Directory.Move(oldNameThumb, newNameThumb);

                RenameFolder renameFolder = new RenameFolder();
                renameFolder.resourceType = paging.type;
                CurrentFolder currentFolder = new CurrentFolder();
                currentFolder.path = paging.currentFolder;
                currentFolder.url = "/" + rootUploads + "/" + paging.type + currentFolder.path;
                currentFolder.acl = 1023;
                renameFolder.currentFolder = currentFolder;
                renameFolder.newName = paging.newFolderName;
                renameFolder.newPath = "/" + paging.newFolderName + "/";
                renameFolder.renamed = 1;
                return Ok(renameFolder);
            }

            return Ok();
        }

    }
}