using Bll.Services.FileStorageService.Files.Base;
using Microsoft.AspNetCore.Http;
using Utility;

namespace Bll.Services.FileStorageService.Files.Concrete
{
    public class ExcelFile : IFile
    {
        public IFormFile File { get; set; }
        public string Identifier { get; private set; }
        public string FileName { get; set; }
        public string OriginalFileName => File.FileName;
        public long Size => File.Length;
        public string FileType => "Excel";
        public string MimeType => File.ContentType;
        public ExcelFile(IFormFile file)
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
            if (!(fileName.EndsWith(".xlsx") || fileName.EndsWith(".xls")))
            {
                response.code = StatusCodes.Status422UnprocessableEntity;
                response.messages = new List<string> { "Invalid Excel file format. Only .xlsx and .xls files are allowed." };
                return response;
            }
            var contentType = File.ContentType?.ToLowerInvariant();
            if (!(contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
               || contentType == "application/vnd.ms-excel"))
            {
                response.code = StatusCodes.Status422UnprocessableEntity;
                response.messages = new List<string> { "Invalid Excel file MIME type." };
                return response;
            }
            await Task.CompletedTask;
            response.code = StatusCodes.Status200OK;
            response.messages = new List<string> { "Excel is valid." };
            return response;
        }
    }
}
