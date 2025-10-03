using Bll.Services.FileStorageService.Context.Concrete.Root;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Storage.Concrete.Root;
namespace Bll.Services.FileStorageService.Factories.Concrete.Root
{
    public class RootImageDeleteFactory : IFileDeleteStorageFactory<RootFileDeleteContext>
    {
        public IFileDeleteStorage<RootFileDeleteContext> CreateStorage()
        {
            return new RootFileDeleteStorage();
        }
    }
}
