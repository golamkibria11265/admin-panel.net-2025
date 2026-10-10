using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Utility;
using Utility.Exceptions;
using static Utility.EnumHelper;

namespace Bll.Services.CommonService
{
    public interface IFileWriterService
    {
        /// <summary>Saves an uploaded menu image and returns the generated file name. Throws CustomApplicationException if the file is not allowed.</summary>
        Task<string> UploadImage(IFormFile file);
        bool DeleteImageByFileName(string fileName);
        string? ImageUrlByFileName(string fileName);
    }

    public class FileWriterService : IFileWriterService
    {
        private const long MaxUploadBytes = 2 * 1024 * 1024; // 2 MB is plenty for a menu icon
        private const string ImageFolderName = "images";
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _logger;

        public FileWriterService
        (
            IWebHostEnvironment hostingEnv,
            IConfiguration configuration,
            Serilog.ILogger logger
        )
        {
            _hostingEnv = hostingEnv;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new CustomApplicationException("The uploaded file is empty.");
            }
            if (file.Length > MaxUploadBytes)
            {
                throw new CustomApplicationException("The image must be 2 MB or smaller.");
            }

            // Decide the type from the file CONTENT (magic bytes), never from the name the user sent.
            byte[] header = new byte[16];
            int read;
            await using (var stream = file.OpenReadStream())
            {
                read = await stream.ReadAsync(header.AsMemory(0, header.Length));
            }
            string? extension = ProjectHelper.GetImageFormat(header.Take(read).ToArray()) switch
            {
                ImageFormat.png => ".png",
                ImageFormat.jpeg => ".jpg",
                ImageFormat.gif => ".gif",
                _ => null // PDF, SVG, HTML, ... are not allowed as menu images
            };
            if (extension == null)
            {
                throw new CustomApplicationException("Only PNG, JPEG or GIF images are allowed.");
            }

            string fileName = Guid.NewGuid().ToString("N") + extension;
            string folder = ImageFolder();
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, fileName);
            await using (var bits = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                await file.CopyToAsync(bits);
            }
            return fileName;
        }

        public bool DeleteImageByFileName(string fileName)
        {
            string? path = SafeImagePath(fileName);
            if (path == null)
            {
                return false;
            }
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not delete image {FileName}", fileName);
                return false;
            }
        }

        public string? ImageUrlByFileName(string fileName)
        {
            var baseUrl = (_configuration.GetValue<string>("AppBaseUrl") ?? "/images").TrimEnd('/');
            string? path = SafeImagePath(fileName);
            if (path == null || !File.Exists(path))
            {
                var defaultEmptyImageName = _configuration.GetValue<string>("DefaultEmptyImageName");
                return string.IsNullOrEmpty(defaultEmptyImageName) ? null : baseUrl + "/" + Uri.EscapeDataString(defaultEmptyImageName);
            }
            return baseUrl + "/" + Uri.EscapeDataString(fileName);
        }

        private string ImageFolder() => Path.Combine(_hostingEnv.WebRootPath, ImageFolderName);

        /// <summary>
        /// Returns the full path only for a plain file name inside wwwroot/images (no "..", no folders).
        /// Paths are NOT lower-cased: Linux file systems are case-sensitive.
        /// </summary>
        private string? SafeImagePath(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
            {
                return null;
            }
            string folder = Path.GetFullPath(ImageFolder());
            string fullPath = Path.GetFullPath(Path.Combine(folder, fileName));
            return fullPath.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.Ordinal) ? fullPath : null;
        }
    }
}
