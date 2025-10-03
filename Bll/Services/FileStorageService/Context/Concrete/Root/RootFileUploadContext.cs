using Bll.Services.FileStorageService.Context.Base;
using Bll.Services.FileStorageService.Files.Base;

namespace Bll.Services.FileStorageService.Context.Concrete.Root
{
    public class RootFileUploadContext : FileContext
    {
        public IFile? File { get; set; }
        public string? DestinationPath { get; set; }
        public string? Identifier { get; set; }
    }
}
