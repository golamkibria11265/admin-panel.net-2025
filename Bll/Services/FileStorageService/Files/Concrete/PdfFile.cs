using Bll.Services.FileStorageService.Files.Base;
using Microsoft.AspNetCore.Http;
using Utility;

namespace Bll.Services.FileStorageService.Files.Concrete
{
    public class PdfFile : IFile
    {
        public IFormFile File { get; set; }
        public string Identifier { get; private set; }
        public string FileName { get; set; }
        public string OriginalFileName => File.FileName;
        public long Size => File.Length;
        public string FileType => "Pdf";
        public string MimeType => File.ContentType;
        public PdfFile(IFormFile file)
        {
            File = file;
            FileName = file.FileName;
            Identifier = Guid.NewGuid().ToString() + "." + File.FileName.Split('.')[File.FileName.Split('.').Length - 1];
        }
        public async Task<CommonResponse> ValidateAsync()
        {
            var response = new CommonResponse();
            if (File == null || string.IsNullOrWhiteSpace(File.FileName))
            {
                response.code = StatusCodes.Status400BadRequest;
                response.messages = new List<string> { "No file provided or filename is missing." };
                return response;
            }
            var fileName = File.FileName.ToLowerInvariant();
            if (!fileName.EndsWith(".pdf"))
            {
                response.code = StatusCodes.Status422UnprocessableEntity;
                response.messages = new List<string> { "Invalid PDF file format. Only .pdf files are allowed." };
                return response;
            }
            var contentType = File.ContentType?.ToLowerInvariant();
            if (contentType != "application/pdf")
            {
                response.code = StatusCodes.Status422UnprocessableEntity;
                response.messages = new List<string> { "Invalid PDF file MIME type." };
                return response;
            }
            await Task.CompletedTask;
            response.code = StatusCodes.Status200OK;
            response.messages = new List<string> { "Pdf is valid." };
            return response;
        }
    }
}
