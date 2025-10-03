using Bll.Services.FileStorageService.Context.Concrete.Root;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Storage.Concrete.Root;

namespace Bll.Services.FileStorageService.Factories.Concrete.Root
{
    public class RootImageExistFactory : IFileExistStorageFactory<RootFileExistContext>
    {
        public IFileExistStorage<RootFileExistContext> CreateStorage()
        {
            return new RootFileExistStorage();
        }
    }
}
