using Microsoft.AspNetCore.Http;
using Utility;

namespace Bll.Services.FileStorageService.Files.Base
{
    public interface IFile
    {
        IFormFile File { get; set; }
        string Identifier { get; }
        string FileName { get; set; }
        string OriginalFileName { get; }
        long Size { get; }
        string FileType { get; }
        string MimeType { get; }
        Task<CommonResponse> ValidateAsync();
    }
}
