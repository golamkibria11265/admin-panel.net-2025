using Dll.Data.Entities;
using Dll.Data;
using Utility.Services.RedisService;

namespace Dll.Repositories.Ap
{
    public interface IApRoleRepository : IRepositoryBase<ApRole>
    {
    }
    public class RoleRepository : RepositoryBase<ApRole>, IApRoleRepository
    {
        private readonly DotNetContext _context;
        private readonly IRedisService _iRedisService;
        public RoleRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, "RoleRepository")
        {
            _context = dotNetContext;
            _iRedisService = iRedisService;
        }
    }
}
