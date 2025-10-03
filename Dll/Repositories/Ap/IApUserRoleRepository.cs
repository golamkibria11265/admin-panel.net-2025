using Dll.Data;
using Dll.Data.Entities;
using Utility.Services.RedisService;

namespace Dll.Repositories.Ap
{
    public interface IApUserRoleRepository : IRepositoryBase<ApUserRole>
    {
    }
    public class UserRoleRepository : RepositoryBase<ApUserRole>, IApUserRoleRepository
    {
        private readonly DotNetContext _context;
        private readonly IRedisService _iRedisService;
        public UserRoleRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, "UserRoleRepository")
        {
            _context = dotNetContext;
            _iRedisService = iRedisService;
        }
    }
}
