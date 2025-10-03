using Dll.Data.Entities;
using Dll.Data;
using Utility.Services.RedisService;

namespace Dll.Repositories.Ap
{
    public interface IApRolePermissionRepository : IRepositoryBase<ApRolePermission>
    {
    }
    public class RolePermissionRepository : RepositoryBase<ApRolePermission>, IApRolePermissionRepository
    {
        private readonly DotNetContext _context;
        private readonly IRedisService _iRedisService;
        public RolePermissionRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, "RolePermissionRepository")
        {
            _context = dotNetContext;
            _iRedisService = iRedisService;
        }
    }
}
