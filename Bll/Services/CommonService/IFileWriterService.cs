using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Utility;

namespace Bll.Services.CommonService
{
    public interface IFileWriterService
    {
        Task<string> UploadImage(IFormFile file);
        bool DeleteImageByFileName(string fileName);
        string? ImageUrlByFileName(string fileName);
    }
    public class FileWriterService : IFileWriterService
    {
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly IConfiguration _configuration;
        public FileWriterService
        (
            IWebHostEnvironment hostingEnv,
            IConfiguration configuration
        )
        {
            _hostingEnv = hostingEnv;
            _configuration = configuration;
        }

        public async Task<string> UploadImage(IFormFile file)
        {
            if (CheckIfImageOrPDFGFile(file))
            {
                return await WriteFile(file);
            }
            return "Invalid image or PDF file";
        }
        public bool DeleteImageByFileName(string fileName)
        {
            var isDeleted = false;
            try
            {
                var path = Path.Combine(_hostingEnv.WebRootPath, "images", fileName).ToLower();
                using (var bits = new FileStream(path, FileMode.Create))
                {
                    bits.Close();
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
                return isDeleted;
            }
            isDeleted = true;
            return isDeleted;
        }
        public string? ImageUrlByFileName(string fileName)
        {
            var path = Path.Combine(_hostingEnv.WebRootPath, "images", fileName).ToLower();
            if (!File.Exists(path))
            {
                var baseUrl = _configuration.GetValue<string>("AppBaseUrl");
                var defaultEmptyImageName = _configuration.GetValue<string>("DefaultEmptyImageName");
                var imageUrl = baseUrl + "/" + defaultEmptyImageName;
                return imageUrl;
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                var baseUrl = _configuration.GetValue<string>("AppBaseUrl");
                var imageUrl = baseUrl + "/" + fileName;
                return imageUrl;
            }
            return null;
        }
        private bool CheckIfImageOrPDFGFile(IFormFile file)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }
            return ProjectHelper.GetImageFormat(fileBytes) != EnumHelper.ImageFormat.unknown;
        }
        private async Task<string> WriteFile(IFormFile file)
        {
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = Guid.NewGuid().ToString() + extension;
                var path = Path.Combine(_hostingEnv.WebRootPath, "images", fileName).ToLower();
                using (var bits = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(bits);
                    bits.Close();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return fileName;
        }



        public async Task<string> UploadeDeleteImage(IFormFile file, string DeteleFileName, bool IsGarbage)
        {
            if (CheckIfImageOrPDFGFile(file))
            {
                return await WriteDeleteFile(file, DeteleFileName, IsGarbage);
            }
            return "Invalid image or PDF file";
        }
        public async Task<string> WriteDeleteFile(IFormFile file, string DeteleFileName, bool IsGarbage)
        {
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = Guid.NewGuid().ToString() + extension;
                var path = Path.Combine(_hostingEnv.WebRootPath, "images", fileName).ToLower();
                using (var bits = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(bits);
                    bits.Close();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return fileName;
        }
    }
}
