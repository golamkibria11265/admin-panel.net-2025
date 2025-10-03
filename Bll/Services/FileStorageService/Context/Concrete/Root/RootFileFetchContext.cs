using Bll.Services.FileStorageService.Context.Base;

namespace Bll.Services.FileStorageService.Context.Concrete.Root
{
    public class RootFileFetchContext : FileContext
    {
        public string? Identifier { get; set; }
    }
}
