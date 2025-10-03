using Dll.Data;
using Dll.Data.Entities;
using Utility.Services.RedisService;

namespace Dll.Repositories.Ap
{
    public interface IApPermissionRepository : IRepositoryBase<ApPermission>
    {
    }
    public class PermissionRepository : RepositoryBase<ApPermission>, IApPermissionRepository
    {
        private readonly DotNetContext _context;
        private readonly IRedisService _iRedisService;
        public PermissionRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, "PermissionRepository")
        {
            _context = dotNetContext;
            _iRedisService = iRedisService;
        }
    }
}
