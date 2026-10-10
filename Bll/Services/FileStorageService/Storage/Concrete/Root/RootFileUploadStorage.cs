using Bll.Services.FileStorageService.Context.Concrete.Root;
using Bll.Services.FileStorageService.Storage.Base;
using Microsoft.AspNetCore.Http;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Concrete.Root
{
    public class RootFileUploadStorage : IFileUploadStorage<RootFileUploadContext>
    {
        public async Task<CommonResponse> UploadAsync(RootFileUploadContext context)
        {
            CommonResponse commonResponse = new();
            try
            {
                if (context.File is null)
                {
                    commonResponse.code = StatusCodes.Status400BadRequest;
                    commonResponse.messages = new List<string> { "No file provided" };
                    return commonResponse;
                }
                if (context.DestinationPath is null)
                {
                    commonResponse.code = StatusCodes.Status400BadRequest;
                    commonResponse.messages = new List<string> { "No destination path provided" };
                    return commonResponse;
                }
                if (string.IsNullOrWhiteSpace(context.File.Identifier))
                {
                    commonResponse.code = StatusCodes.Status400BadRequest;
                    commonResponse.messages = new List<string> { "File identifier is missing" };
                    return commonResponse;
                }
                if (!Directory.Exists(context.DestinationPath))
                {
                    Directory.CreateDirectory(context.DestinationPath);
                }
                // No ToLower(): Linux paths are case-sensitive. GetFileName() blocks "../" in the identifier.
                var path = Path.Combine(context.DestinationPath, Path.GetFileName(context.File.Identifier));
                using (var bits = new FileStream(path, FileMode.Create))
                {
                    await context.File.File.CopyToAsync(bits);
                    bits.Close();
                }
                commonResponse.code = StatusCodes.Status200OK;
                commonResponse.messages = new List<string> { "Success" };
            }
            catch (Exception ex)
            {
                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { "File upload failed" };
            }
            return commonResponse;
        }
    }
}
